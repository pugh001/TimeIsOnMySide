namespace Overtime.Data.Entities;

public sealed class OpeningHours
{
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
}
