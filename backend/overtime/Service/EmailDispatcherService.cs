using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Overtime.Service;

public sealed class EmailDispatcherService : BackgroundService
{
    private readonly EmailQueue _queue;
    private readonly INotificationService _notifications;
    private readonly ILogger<EmailDispatcherService> _logger;

    public EmailDispatcherService(
        EmailQueue queue,
        INotificationService notifications,
        ILogger<EmailDispatcherService> logger)
    {
        ArgumentNullException.ThrowIfNull(queue);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(logger);
        _queue = queue;
        _notifications = notifications;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var confirmation in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _notifications.SendBookingConfirmationAsync(confirmation, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Email dispatcher failed for booking {BookingRef}", confirmation.BookingRef);
            }
        }
    }
}
