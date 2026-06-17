using Overtime.Service.Models.Bookings;

namespace Overtime.Service;

public interface IBookingService
{
    Task<CreateBookingResponse> CreateBookingAsync(CreateBookingRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BookingResponse>> GetBookingsAsync(Guid staffId, CancellationToken ct = default);
    Task<IReadOnlyList<BookingResponse>> GetBookingsByUserIdAsync(Guid userId, CancellationToken ct = default);
}
