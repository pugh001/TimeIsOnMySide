namespace Overtime.Service.Models.Bookings;

public sealed record BookingConfirmation(
    string BookingRef,
    string CustomerName,
    string CustomerEmail,
    string StaffEmail,
    DateOnly Date,
    TimeOnly StartTime);
