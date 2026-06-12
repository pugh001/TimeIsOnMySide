namespace Overtime.Service.Models.Auth;

public sealed record LoginResponse(
    string EmployeeName,
    string Role,
    string? AdminToken = null,
    Guid? AdminUserId = null,
    string? StaffToken = null,
    Guid? StaffUserId = null);
