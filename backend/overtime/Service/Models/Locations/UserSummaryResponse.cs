using Overtime.Service.Models.Users;

namespace Overtime.Service.Models.Locations;

public sealed record UserSummaryResponse(
    Guid Id,
    string FullName,
    IReadOnlyList<UserWorkingTimeDto> WorkingTimes);
