namespace Overtime.Service.Models.Users;

public sealed record CreateUserResponse(string UserId, string Username, string? Email);
