using Microsoft.EntityFrameworkCore;
using Overtime.Common.Exceptions;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service.Models.Slots;

namespace Overtime.Service;

public sealed class SlotService : ISlotService
{
    private static readonly TimeOnly[] AllSlotTimes =
        Enumerable.Range(0, 48).Select(i => new TimeOnly(i / 2, (i % 2) * 30)).ToArray();

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
            .Where(t => SlotEnd(t) is {} end && t >= openTime && end <= closeTime)
            .Where(t => SlotEnd(t) is {} end && staffWorkingToday.Any(x => t >= x.Shift!.ShiftStart && end <= x.Shift!.ShiftEnd))
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
                var end = SlotEnd(start) ?? start; // midnight-crossing slots are filtered out before reaching here
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

    // Returns null when the slot would cross midnight — null always fails the <= closeTime filter.
    private static TimeOnly? SlotEnd(TimeOnly start)
    {
        var endMinutes = start.Hour * 60 + start.Minute + 30;
        return endMinutes >= 24 * 60 ? null : new TimeOnly(endMinutes / 60, endMinutes % 60);
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
