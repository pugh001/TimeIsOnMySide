namespace Overtime.Service.Models.Locations;

public sealed class CreateLocationRequest
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public Dictionary<string, OpeningHoursDto?>? OpeningHours { get; init; }
}
