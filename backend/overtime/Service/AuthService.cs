using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Auth;

namespace Overtime.Service;

public sealed class AuthService : IAuthService
{
    private readonly OvertimeDbContext _db;
    private readonly IPasswordHasher<UserEntity> _hasher;
    private readonly IAdminTokenService _adminTokenService;

    public AuthService(OvertimeDbContext db, IPasswordHasher<UserEntity> hasher, IAdminTokenService adminTokenService)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(hasher);
        ArgumentNullException.ThrowIfNull(adminTokenService);
        _db = db;
        _hasher = hasher;
        _adminTokenService = adminTokenService;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

        if (user is null)
            return null;

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;

        return user.Role switch
        {
            "admin" => new LoginResponse(
                user.FullName, user.Role,
                AdminToken: _adminTokenService.GenerateToken(user.Id),
                AdminUserId: user.Id,
                StaffToken: null,
                StaffUserId: null),
            "staff" => new LoginResponse(
                user.FullName, user.Role,
                AdminToken: null,
                AdminUserId: null,
                StaffToken: _adminTokenService.GenerateToken(user.Id),
                StaffUserId: user.Id),
            _ => new LoginResponse(user.FullName, user.Role, null, null, null, null)
        };
    }
}
