using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Overtime.Service;
using Overtime.Service.Models.Bookings;
using Overtime.Service.Models.Email;

namespace Overtime.Tests.Unit.Service;

public sealed class NotificationServiceTests
{
    private static BookingConfirmation SampleConfirmation() => new(
        BookingRef: "bk-test1234",
        CustomerName: "Jane Doe",
        CustomerEmail: "jane@example.com",
        StaffEmail: "staff@example.com",
        Date: new DateOnly(2026, 6, 15),
        StartTime: new TimeOnly(10, 0));

    private static IOptions<EmailOptions> BuildOptions(string host = "127.0.0.1", int port = 19999) =>
        Options.Create(new EmailOptions
        {
            SmtpHost = host,
            SmtpPort = port,
            Username = "",
            Password = "",
            FromAddress = "noreply@test.local",
            FromName = "Test",
            DemoRecipientEmail = "demo@test.local"
        });

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        var logger = Substitute.For<ILogger<NotificationService>>();
        Action act = () => new NotificationService(null!, logger);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBookingConfirmationAsync_NullConfirmation_ThrowsArgumentNullException()
    {
        var logger = Substitute.For<ILogger<NotificationService>>();
        var svc = new NotificationService(BuildOptions(), logger);

        Func<Task> act = () => svc.SendBookingConfirmationAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task SendBookingConfirmationAsync_SmtpUnreachable_DoesNotThrow()
    {
        var logger = Substitute.For<ILogger<NotificationService>>();
        var svc = new NotificationService(BuildOptions(), logger);

        Func<Task> act = () => svc.SendBookingConfirmationAsync(SampleConfirmation());
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SendBookingConfirmationAsync_SmtpUnreachable_LogsError()
    {
        var logger = Substitute.For<ILogger<NotificationService>>();
        var svc = new NotificationService(BuildOptions(), logger);

        await svc.SendBookingConfirmationAsync(SampleConfirmation());

        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
