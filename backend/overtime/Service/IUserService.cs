using Overtime.Service.Models.Users;

namespace Overtime.Service;

public interface IUserService
{
    Task<CreateUserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
}
