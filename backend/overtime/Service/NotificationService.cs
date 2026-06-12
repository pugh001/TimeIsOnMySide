using System.Diagnostics;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Overtime.Service.Models.Bookings;
using Overtime.Service.Models.Email;

namespace Overtime.Service;

public sealed class NotificationService : INotificationService
{
    private readonly EmailOptions _options;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(IOptions<EmailOptions> options, ILogger<NotificationService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendBookingConfirmationAsync(BookingConfirmation confirmation, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        try
        {
            var customerTo = string.IsNullOrWhiteSpace(_options.DemoRecipientEmail)
                ? confirmation.CustomerEmail
                : _options.DemoRecipientEmail;
            var staffTo = string.IsNullOrWhiteSpace(_options.DemoRecipientEmail)
                ? confirmation.StaffEmail
                : _options.DemoRecipientEmail;

            // SMTP connection per send: ~50ms connect + ~20ms auth = ~70ms overhead per booking.
            // Acceptable at current MVP scale (<100 bookings/day). For higher throughput, use a send queue.
            using var client = new SmtpClient();
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.Auto, ct);
            Debug.Assert(client.IsConnected, "SMTP client must be connected before send.");

            if (!string.IsNullOrWhiteSpace(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password, ct);

            var customerMsg = BuildCustomerMessage(confirmation, _options.FromAddress, _options.FromName, customerTo);
            await client.SendAsync(customerMsg, ct);

            if (!string.IsNullOrWhiteSpace(staffTo))
            {
                var staffMsg = BuildStaffMessage(confirmation, _options.FromAddress, _options.FromName, staffTo);
                await client.SendAsync(staffMsg, ct);
            }

            await client.DisconnectAsync(true, ct);
            Debug.Assert(!client.IsConnected, "SMTP client must be disconnected after send.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send booking confirmation emails for booking {BookingRef}", confirmation.BookingRef);
        }
    }

    private static MimeMessage BuildCustomerMessage(
        BookingConfirmation c, string fromAddress, string fromName, string toAddress)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(fromName, fromAddress));
        msg.To.Add(MailboxAddress.Parse(toAddress));
        msg.Subject = "Your appointment is confirmed";
        msg.Body = new TextPart("plain")
        {
            Text = $"""
                Hi {c.CustomerName},

                Your appointment has been confirmed.

                Booking reference: {c.BookingRef}
                Date: {c.Date:dddd, d MMMM yyyy}
                Time: {c.StartTime:HH:mm}

                We look forward to seeing you.

                TimeIsOnMySide
                """
        };
        return msg;
    }

    private static MimeMessage BuildStaffMessage(
        BookingConfirmation c, string fromAddress, string fromName, string toAddress)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(fromName, fromAddress));
        msg.To.Add(MailboxAddress.Parse(toAddress));
        msg.Subject = "New appointment booked";
        msg.Body = new TextPart("plain")
        {
            Text = $"""
                You have a new appointment.

                Booking reference: {c.BookingRef}
                Customer: {c.CustomerName}
                Date: {c.Date:dddd, d MMMM yyyy}
                Time: {c.StartTime:HH:mm}

                TimeIsOnMySide
                """
        };
        return msg;
    }
}
