namespace Overtime.Service.Models.Bookings;

public sealed record BookingResponse(
    string BookingRef,
    string Date,
    string StartTime,
    string EndTime,
    string CustomerName);
