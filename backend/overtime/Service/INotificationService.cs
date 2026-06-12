using Overtime.Service.Models.Bookings;

namespace Overtime.Service;

public interface INotificationService
{
    Task SendBookingConfirmationAsync(BookingConfirmation confirmation, CancellationToken ct = default);
}
