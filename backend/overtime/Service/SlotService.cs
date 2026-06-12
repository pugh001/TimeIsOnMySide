using Microsoft.EntityFrameworkCore;
using Overtime.Common.Exceptions;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Slots;

namespace Overtime.Service;

public sealed class SlotService : ISlotService
{
    private static readonly TimeOnly[] AllSlotTimes =
    [
        new(9, 0), new(9, 30), new(10, 0), new(10, 30),
        new(11, 0), new(11, 30), new(12, 0), new(12, 30),
        new(13, 0), new(13, 30), new(14, 0), new(14, 30),
        new(15, 0), new(15, 30), new(16, 0), new(16, 30)
    ];

    private readonly OvertimeDbContext _db;

    public SlotService(OvertimeDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<IReadOnlyList<SlotResponse>> GetSlotsAsync(
        string date, string locationId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(date);
        ArgumentException.ThrowIfNullOrEmpty(locationId);

        if (!DateOnly.TryParse(date, out var parsedDate))
            throw new ArgumentException($"Invalid date format: '{date}'.", nameof(date));

        if (!Guid.TryParse(locationId, out var locationGuid))
            throw new ArgumentException($"Invalid locationId format: '{locationId}'.", nameof(locationId));

        var location = await _db.Locations
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == locationGuid, ct)
            ?? throw new NotFoundException("Location", locationId);

        var dayHours = GetDayHours(location.OpeningHours, parsedDate.DayOfWeek);

        if (dayHours?.OpenTime is null || dayHours.CloseTime is null)
            return [];

        var openTime = dayHours.OpenTime.Value;
        var closeTime = dayHours.CloseTime.Value;

        var dayName = parsedDate.DayOfWeek.ToString().ToLowerInvariant();
        var staffWorkingTimes = await _db.Users
            .AsNoTracking()
            .Where(u => u.LocationId == location.Id && u.Role == "staff")
            .Select(u => u.WorkingTimes)
            .ToListAsync(ct);

        var staffWorkingToday = staffWorkingTimes
            .Select(times => times.FirstOrDefault(w => string.Equals(w.Day, dayName, StringComparison.OrdinalIgnoreCase)))
            .Where(shift => shift is not null)
            .Select(shift => new { Shift = shift })
            .ToList();

        if (staffWorkingToday.Count == 0)
            return [];

        var eligibleTimes = AllSlotTimes
            .Where(t => t >= openTime && t.AddMinutes(30) <= closeTime)
            .Where(t => staffWorkingToday.Any(x => t >= x.Shift!.ShiftStart && t.AddMinutes(30) <= x.Shift!.ShiftEnd))
            .ToArray();


        var bookedCounts = await _db.Bookings
            .AsNoTracking()
            .Where(b => b.Staff.LocationId == location.Id && b.SlotDate == parsedDate)
            .GroupBy(b => b.StartTime)
            .Select(g => new { StartTime = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StartTime, x => x.Count, ct);

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);
        var nowTime = parsedDate == today
            ? (TimeOnly?)TimeOnly.FromDateTime(now)
            : null;

        return GenerateSlots(date, location.Slug, eligibleTimes, bookedCounts, staffWorkingToday.Count, nowTime);

    }

    internal static IReadOnlyList<SlotResponse> GenerateSlots(
        string date,
        string locationSlug,
        IEnumerable<TimeOnly> eligibleSlotTimes,
        Dictionary<TimeOnly, int> bookedCounts,
        int activeStaffCount,
        TimeOnly? nowTime = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(date);
        ArgumentException.ThrowIfNullOrEmpty(locationSlug);

        if (!DateOnly.TryParse(date, out _))
            throw new ArgumentException($"Invalid date format: '{date}'.", nameof(date));

        return eligibleSlotTimes
            .OrderBy(t => t)
            .Select(start =>
            {
                var end = start.AddMinutes(30);
                bookedCounts.TryGetValue(start, out var booked);
                var isPast = nowTime.HasValue && start < nowTime.Value;
                var status = (isPast || (activeStaffCount > 0 && booked >= activeStaffCount))
                    ? "unavailable"
                    : "available";

                return new SlotResponse(
                    Id: $"{locationSlug}-{date}-{start:HH\\:mm}",
                    Date: date,
                    StartTime: start.ToString("HH:mm"),
                    EndTime: end.ToString("HH:mm"),
                    Status: status,
                    LocationId: locationSlug);
            }).ToList();
    }

    private static OpeningHours? GetDayHours(LocationOpeningHours? hours, DayOfWeek day) =>
        hours is null ? null : day switch
        {
            DayOfWeek.Monday    => hours.Monday,
            DayOfWeek.Tuesday   => hours.Tuesday,
            DayOfWeek.Wednesday => hours.Wednesday,
            DayOfWeek.Thursday  => hours.Thursday,
            DayOfWeek.Friday    => hours.Friday,
            DayOfWeek.Saturday  => hours.Saturday,
            DayOfWeek.Sunday    => hours.Sunday,
            _ => null
        };
}
