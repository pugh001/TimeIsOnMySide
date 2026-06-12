namespace Overtime.Service.Models.Bookings;

public sealed class CreateBookingRequest
{
    public string SlotId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
