using Overtime.Service.Models.Locations;

namespace Overtime.Service;

public interface ILocationService
{
    Task<IReadOnlyList<LocationResponse>> GetLocationsAsync(CancellationToken ct = default);
    Task<LocationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CreateLocationResponse> CreateAsync(CreateLocationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<UserSummaryResponse>> GetUsersForLocationAsync(Guid locationId, CancellationToken ct = default);
}
