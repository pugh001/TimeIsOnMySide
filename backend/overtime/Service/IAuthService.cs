using Overtime.Service.Models.Auth;

namespace Overtime.Service;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
