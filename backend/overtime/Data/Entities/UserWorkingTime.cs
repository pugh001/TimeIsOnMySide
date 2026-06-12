namespace Overtime.Data.Entities;

public sealed class UserWorkingTime
{
    public string Day { get; init; } = string.Empty;
    public TimeOnly ShiftStart { get; init; }
    public TimeOnly ShiftEnd { get; init; }
}
