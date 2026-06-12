using System.Diagnostics;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Users;

namespace Overtime.Service;

public sealed class UserService : IUserService
{
    private readonly OvertimeDbContext _db;
    private readonly IPasswordHasher<UserEntity> _hasher;

    public UserService(OvertimeDbContext db, IPasswordHasher<UserEntity> hasher)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(hasher);
        _db = db;
        _hasher = hasher;
    }

    public async Task<CreateUserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!Guid.TryParse(request.LocationId, out var locationGuid))
            throw new ValidationException([new ValidationFailure("locationId", $"Location '{request.LocationId}' not found.")]);

        var locationExists = await _db.Locations.AnyAsync(l => l.Id == locationGuid, ct);
        if (!locationExists)
            throw new ValidationException([new ValidationFailure("locationId", $"Location '{request.LocationId}' not found.")]);

        var username = await GenerateUsernameAsync(request.FirstName, ct);

        var entity = new UserEntity();
        var passwordHash = _hasher.HashPassword(entity, request.Password);

        entity.Id = Guid.NewGuid();
        entity.Username = username;
        entity.PasswordHash = passwordHash;
        entity.Role = "staff";
        entity.FullName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";
        entity.FirstName = request.FirstName.Trim();
        entity.LastName = request.LastName.Trim();
        entity.LocationId = locationGuid;
        entity.WorkingTimes = request.WorkingTimes
            .Select(w => new UserWorkingTime
            {
                Day = w.Day.ToLowerInvariant(),
                ShiftStart = TimeOnly.Parse(w.ShiftStart),
                ShiftEnd = TimeOnly.Parse(w.ShiftEnd)
            })
            .ToList();
        entity.Email = request.Email;
        entity.CreatedAt = DateTimeOffset.UtcNow;

        _db.Users.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CreateUserResponse(entity.Id.ToString(), entity.Username, entity.Email);
    }

    private async Task<string> GenerateUsernameAsync(string firstName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        var prefix = firstName.Trim().ToLowerInvariant();

        // Pull only the fixed-length suffix candidates: prefix length + 4 digits.
        // Take(10000) bounds memory use; the sequence cap of 9999 makes this unreachable in practice.
        var suffixes = await _db.Users
            .Where(u => u.Username.StartsWith(prefix) && u.Username.Length == prefix.Length + 4)
            .Select(u => u.Username.Substring(prefix.Length))
            .Take(10000)
            .ToListAsync(ct);

        var maxSuffix = suffixes
            .Where(s => s.Length == 4 && s.All(char.IsDigit))
            .Select(s => int.Parse(s))
            .DefaultIfEmpty(0)
            .Max();

        var next = maxSuffix + 1;
        Debug.Assert(next <= 9999, $"Username sequence exhausted for prefix '{prefix}'.");
        if (next > 9999)
            throw new InvalidOperationException($"Username sequence exhausted for prefix '{prefix}'.");

        return $"{prefix}{next:D4}";
    }
}
