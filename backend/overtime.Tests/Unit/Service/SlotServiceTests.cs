using FluentAssertions;
using Overtime.Service;

namespace Overtime.Tests.Unit.Service;

public sealed class SlotServiceTests
{
    private const string WeekdayDate = "2026-05-26"; // Tuesday
    private const string SaturdayDate = "2026-05-23";
    private const string SundayDate = "2026-05-24";
    private const string LocationSlug = "branch-city-a";

    // Standard eligible times: full 09:00–17:00 window (16 slots)
    private static readonly TimeOnly[] FullDaySlots =
    [
        new(9, 0), new(9, 30), new(10, 0), new(10, 30),
        new(11, 0), new(11, 30), new(12, 0), new(12, 30),
        new(13, 0), new(13, 30), new(14, 0), new(14, 30),
        new(15, 0), new(15, 30), new(16, 0), new(16, 30)
    ];

    // ── Slot generation across any day of week ────────────────────────────────

    [Fact]
    public void GenerateSlots_WeekdayDate_Returns16Slots()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().HaveCount(16);
    }

    [Fact]
    public void GenerateSlots_Saturday_Returns16Slots_WhenEligibleTimesProvided()
    {
        // Weekend no longer hard-blocked — opening-hours layer above handles closed days
        var slots = SlotService.GenerateSlots(SaturdayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().HaveCount(16);
    }

    [Fact]
    public void GenerateSlots_Sunday_Returns16Slots_WhenEligibleTimesProvided()
    {
        var slots = SlotService.GenerateSlots(SundayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().HaveCount(16);
    }

    [Fact]
    public void GenerateSlots_Saturday_ReturnsCorrectDate()
    {
        var slots = SlotService.GenerateSlots(SaturdayDate, LocationSlug, FullDaySlots, [], 1);
        slots.Should().OnlyContain(s => s.Date == SaturdayDate);
    }

    // ── Time window correctness ───────────────────────────────────────────────

    [Fact]
    public void GenerateSlots_FirstSlotStartsAt0900()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots[0].StartTime.Should().Be("09:00");
    }

    [Fact]
    public void GenerateSlots_LastSlotStartsAt1630()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots[15].StartTime.Should().Be("16:30");
    }

    [Fact]
    public void GenerateSlots_EachSlotEndTimeIs30MinAfterStart()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots[0].EndTime.Should().Be("09:30");
        slots[15].EndTime.Should().Be("17:00");
    }

    // ── Slot ID format ────────────────────────────────────────────────────────

    [Fact]
    public void GenerateSlots_SlotIdMatchesExpectedFormat()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots[0].Id.Should().Be("branch-city-a-2026-05-26-09:00");
        slots[15].Id.Should().Be("branch-city-a-2026-05-26-16:30");
    }

    [Fact]
    public void GenerateSlots_SlotLocationIdMatchesInput()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().OnlyContain(s => s.LocationId == LocationSlug);
    }

    [Fact]
    public void GenerateSlots_SlotDateMatchesInput()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().OnlyContain(s => s.Date == WeekdayDate);
    }

    // ── Availability logic ────────────────────────────────────────────────────

    [Fact]
    public void GenerateSlots_NoBookings_AllSlotsAvailable()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2);
        slots.Should().OnlyContain(s => s.Status == "available");
    }

    [Fact]
    public void GenerateSlots_ZeroActiveStaff_AllSlotsAvailable()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 0);
        slots.Should().OnlyContain(s => s.Status == "available");
    }

    [Fact]
    public void GenerateSlots_AllStaffBookedAt0900_ThatSlotUnavailable()
    {
        var booked = new Dictionary<TimeOnly, int> { [new TimeOnly(9, 0)] = 2 };
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, booked, 2);

        slots.First(s => s.StartTime == "09:00").Status.Should().Be("unavailable");
        slots.Where(s => s.StartTime != "09:00").Should().OnlyContain(s => s.Status == "available");
    }

    [Fact]
    public void GenerateSlots_OneOf2StaffBookedAt0900_ThatSlotStillAvailable()
    {
        var booked = new Dictionary<TimeOnly, int> { [new TimeOnly(9, 0)] = 1 };
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, booked, 2);

        slots.First(s => s.StartTime == "09:00").Status.Should().Be("available");
    }

    [Fact]
    public void GenerateSlots_AllSlotsFullyBooked_AllSlotsUnavailable()
    {
        var booked = FullDaySlots.ToDictionary(t => t, _ => 1);
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, booked, 1);
        slots.Should().OnlyContain(s => s.Status == "unavailable");
    }

    // ── Eligible slot filtering ───────────────────────────────────────────────

    [Fact]
    public void GenerateSlots_EmptyEligibleTimes_ReturnsEmpty()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, [], [], 2);
        slots.Should().BeEmpty();
    }

    [Fact]
    public void GenerateSlots_OnlyMorningSlots_OnlyMorningSlotsReturned()
    {
        TimeOnly[] morningSlots = [new(9, 0), new(9, 30), new(10, 0)];
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, morningSlots, [], 1);

        slots.Should().HaveCount(3);
        slots.Select(s => s.StartTime).Should().BeEquivalentTo(["09:00", "09:30", "10:00"]);
    }

    [Fact]
    public void GenerateSlots_EligibleTimesAreSortedInOutput()
    {
        TimeOnly[] unsorted = [new(10, 0), new(9, 0), new(9, 30)];
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, unsorted, [], 1);

        slots.Select(s => s.StartTime).Should().ContainInOrder("09:00", "09:30", "10:00");
    }

    // ── Guard conditions ──────────────────────────────────────────────────────

    [Fact]
    public void GenerateSlots_NullDate_ThrowsArgumentException()
    {
        var act = () => SlotService.GenerateSlots(null!, LocationSlug, FullDaySlots, [], 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateSlots_NullSlug_ThrowsArgumentException()
    {
        var act = () => SlotService.GenerateSlots(WeekdayDate, null!, FullDaySlots, [], 1);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GenerateSlots_InvalidDateFormat_ThrowsArgumentException()
    {
        var act = () => SlotService.GenerateSlots("not-a-date", LocationSlug, FullDaySlots, [], 1);
        act.Should().Throw<ArgumentException>();
    }

    // ── Past-slot filtering (today only) ─────────────────────────────────────

    [Fact]
    public void GenerateSlots_TodayDate_PastSlots_AreUnavailable()
    {
        var nowTime = new TimeOnly(12, 0);
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2, nowTime);

        slots.Where(s => TimeOnly.Parse(s.StartTime) < nowTime)
             .Should().OnlyContain(s => s.Status == "unavailable");
    }

    [Fact]
    public void GenerateSlots_TodayDate_FutureSlots_AreAvailable()
    {
        var nowTime = new TimeOnly(12, 0);
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2, nowTime);

        slots.Where(s => TimeOnly.Parse(s.StartTime) >= nowTime)
             .Should().OnlyContain(s => s.Status == "available");
    }

    [Fact]
    public void GenerateSlots_TodayDate_SlotExactlyAtNowTime_IsAvailable()
    {
        var nowTime = new TimeOnly(12, 0);
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2, nowTime);

        slots.First(s => s.StartTime == "12:00").Status.Should().Be("available");
    }

    [Fact]
    public void GenerateSlots_NowTimeNull_NoSlotsFilteredAsPast()
    {
        var slots = SlotService.GenerateSlots(WeekdayDate, LocationSlug, FullDaySlots, [], 2, nowTime: null);

        slots.Should().OnlyContain(s => s.Status == "available");
    }
}
