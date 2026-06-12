namespace Overtime.Data.Entities;

public sealed class BookingEntity
{
    public Guid Id { get; set; }
    public string BookingRef { get; set; } = string.Empty;
    public Guid StaffId { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public UserEntity Staff { get; set; } = null!;
}
