namespace Overtime.Service.Models.Locations;

public sealed record LocationResponse(
    Guid Id,
    string Slug,
    string Name,
    string? Address = null,
    Dictionary<string, OpeningHoursDto?>? OpeningHours = null);
