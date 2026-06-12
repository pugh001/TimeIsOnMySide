using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Overtime.Common.Exceptions;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Locations;
using Overtime.Service.Models.Users;

namespace Overtime.Service;

public sealed class LocationService : ILocationService
{
    private readonly OvertimeDbContext _db;

    public LocationService(OvertimeDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<IReadOnlyList<LocationResponse>> GetLocationsAsync(CancellationToken ct = default)
    {
        return await _db.Locations
            .AsNoTracking()
            .OrderBy(l => l.Name)
            .Select(l => new LocationResponse(
                l.Id,
                l.Slug,
                l.Name,
                l.Address,
                l.OpeningHours == null ? null : MapOpeningHours(l.OpeningHours)))
            .ToListAsync(ct);
    }

    public async Task<LocationResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var location = await _db.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (location is null) return null;

        return new LocationResponse(
            location.Id,
            location.Slug,
            location.Name,
            location.Address,
            location.OpeningHours is null ? null : MapOpeningHours(location.OpeningHours));
    }

    public async Task<CreateLocationResponse> CreateAsync(CreateLocationRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var slug = await GenerateUniqueSlugAsync(request.Name, ct);
        var location = new LocationEntity
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = request.Name,
            Address = request.Address,
            OpeningHours = request.OpeningHours is null ? null : MapOpeningHoursEntity(request.OpeningHours),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.Locations.Add(location);
        await _db.SaveChangesAsync(ct);
        Debug.Assert(location.Id != Guid.Empty, "Location ID must be assigned after save.");
        Debug.Assert(!string.IsNullOrEmpty(location.Slug), "Location slug must be set after save.");
        return new CreateLocationResponse(location.Slug, location.Id);
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> GetUsersForLocationAsync(
        Guid locationId, CancellationToken ct = default)
    {
        var locationExists = await _db.Locations.AnyAsync(l => l.Id == locationId, ct);
        if (!locationExists)
            throw new NotFoundException("Location", locationId.ToString());

        var users = await _db.Users
            .AsNoTracking()
            .Where(u => u.LocationId == locationId && u.Role == "staff")
            .OrderBy(u => u.FullName)
            .ToListAsync(ct);

        return users
            .Select(u => new UserSummaryResponse(
                u.Id,
                u.FullName,
                u.WorkingTimes
                    .Select(w => new UserWorkingTimeDto(w.Day, w.ShiftStart.ToString("HH:mm"), w.ShiftEnd.ToString("HH:mm")))
                    .ToList()))
            .ToList();
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken ct)
    {
        var baseSlug = GenerateSlug(name);
        if (!await _db.Locations.AnyAsync(l => l.Slug == baseSlug, ct))
            return baseSlug;

        // Each iteration fires one DB query. Slug collisions are rare in practice
        // (<2 iterations for 99% of location names), so N+1 here is acceptable.
        const int SlugSuffixMax = 100;
        for (var suffix = 2; suffix <= SlugSuffixMax; suffix++)
        {
            var candidate = $"{baseSlug}-{suffix}";
            if (!await _db.Locations.AnyAsync(l => l.Slug == candidate, ct))
                return candidate;
        }

        throw new InvalidOperationException($"Slug sequence exhausted for base '{baseSlug}'.");
    }

    private static string GenerateSlug(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return name.Trim().ToLowerInvariant()
            .Replace(' ', '-')
            .Replace("'", string.Empty)
            .Replace("\"", string.Empty);
    }

    private static Dictionary<string, OpeningHoursDto?> MapOpeningHours(LocationOpeningHours hours)
        => new(StringComparer.OrdinalIgnoreCase)
        {
            ["monday"]    = MapDay(hours.Monday),
            ["tuesday"]   = MapDay(hours.Tuesday),
            ["wednesday"] = MapDay(hours.Wednesday),
            ["thursday"]  = MapDay(hours.Thursday),
            ["friday"]    = MapDay(hours.Friday),
            ["saturday"]  = MapDay(hours.Saturday),
            ["sunday"]    = MapDay(hours.Sunday)
        };

    private static OpeningHoursDto? MapDay(OpeningHours? day)
        => day is null ? null : new OpeningHoursDto(
            day.OpenTime?.ToString("HH:mm"),
            day.CloseTime?.ToString("HH:mm"));

    private static LocationOpeningHours MapOpeningHoursEntity(Dictionary<string, OpeningHoursDto?> dto)
    {
        dto.TryGetValue("monday",    out var mon);
        dto.TryGetValue("tuesday",   out var tue);
        dto.TryGetValue("wednesday", out var wed);
        dto.TryGetValue("thursday",  out var thu);
        dto.TryGetValue("friday",    out var fri);
        dto.TryGetValue("saturday",  out var sat);
        dto.TryGetValue("sunday",    out var sun);

        return new LocationOpeningHours
        {
            Monday    = ParseDay(mon),
            Tuesday   = ParseDay(tue),
            Wednesday = ParseDay(wed),
            Thursday  = ParseDay(thu),
            Friday    = ParseDay(fri),
            Saturday  = ParseDay(sat),
            Sunday    = ParseDay(sun)
        };
    }

    private static OpeningHours? ParseDay(OpeningHoursDto? dto)
    {
        if (dto is null) return null;

        TimeOnly? openTime = null;
        TimeOnly? closeTime = null;

        if (dto.OpenTime is not null)
        {
            if (!TimeOnly.TryParse(dto.OpenTime, out var parsed))
                throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure("openTime", $"'{dto.OpenTime}' is not a valid time (expected HH:mm).")]);
            openTime = parsed;
        }

        if (dto.CloseTime is not null)
        {
            if (!TimeOnly.TryParse(dto.CloseTime, out var parsed))
                throw new FluentValidation.ValidationException([new FluentValidation.Results.ValidationFailure("closeTime", $"'{dto.CloseTime}' is not a valid time (expected HH:mm).")]);
            closeTime = parsed;
        }

        return new OpeningHours { OpenTime = openTime, CloseTime = closeTime };
    }
}
