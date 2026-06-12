namespace Overtime.Service.Models.Users;

public sealed record CreateUserRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string LocationId { get; init; } = string.Empty;
    public string? Email { get; init; }
    public UserWorkingTimeDto[] WorkingTimes { get; init; } = [];
}
