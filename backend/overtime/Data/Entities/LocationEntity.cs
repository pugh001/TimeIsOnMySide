namespace Overtime.Data.Entities;

public sealed class LocationEntity
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public LocationOpeningHours? OpeningHours { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

}
