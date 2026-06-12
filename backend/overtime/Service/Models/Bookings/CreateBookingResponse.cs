namespace Overtime.Service.Models.Bookings;

public sealed record CreateBookingResponse(
    string BookingId,
    string SlotId,
    string StartTime,
    string Date,
    string Name);
