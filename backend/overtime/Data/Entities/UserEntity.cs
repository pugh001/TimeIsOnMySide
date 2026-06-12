namespace Overtime.Data.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    // Nullable: admin seed user and staff created before this field was introduced may have no email.
    public string? Email { get; set; }
    public Guid? LocationId { get; set; }
    public IList<UserWorkingTime> WorkingTimes { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }

    public LocationEntity? Location { get; init; }
}
