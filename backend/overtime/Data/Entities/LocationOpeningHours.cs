namespace Overtime.Data.Entities;

public sealed class LocationOpeningHours
{
    public OpeningHours? Monday { get; set; }
    public OpeningHours? Tuesday { get; set; }
    public OpeningHours? Wednesday { get; set; }
    public OpeningHours? Thursday { get; set; }
    public OpeningHours? Friday { get; set; }
    public OpeningHours? Saturday { get; set; }
    public OpeningHours? Sunday { get; set; }
}
