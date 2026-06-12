using FluentAssertions;
using Overtime.Service;

namespace Overtime.Tests.Unit.Service;

public sealed class BookingServiceParsingTests
{
    // ── ParseSlotId ───────────────────────────────────────────────────────────

    [Fact]
    public void ParseSlotId_ValidSimpleSlug_ExtractsComponents()
    {
        var (slug, date, time) = BookingService.ParseSlotId("branch-city-a-2026-05-26-09:00");
        slug.Should().Be("branch-city-a");
        date.Should().Be(new DateOnly(2026, 5, 26));
        time.Should().Be(new TimeOnly(9, 0));
    }

    [Fact]
    public void ParseSlotId_ValidSlugWithMultipleDashes_ExtractsComponents()
    {
        var (slug, date, time) = BookingService.ParseSlotId("my-long-location-slug-2026-12-31-16:30");
        slug.Should().Be("my-long-location-slug");
        date.Should().Be(new DateOnly(2026, 12, 31));
        time.Should().Be(new TimeOnly(16, 30));
    }

    [Fact]
    public void ParseSlotId_EmptyString_ThrowsArgumentException()
    {
        var act = () => BookingService.ParseSlotId(string.Empty);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ParseSlotId_NullString_ThrowsArgumentException()
    {
        var act = () => BookingService.ParseSlotId(null!);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ParseSlotId_MissingTimePart_ThrowsArgumentException()
    {
        var act = () => BookingService.ParseSlotId("branch-city-a-2026-05-26");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*does not match expected format*");
    }

    [Fact]
    public void ParseSlotId_MissingDatePart_ThrowsArgumentException()
    {
        var act = () => BookingService.ParseSlotId("branch-city-a-09:00");
        act.Should().Throw<ArgumentException>()
            .WithMessage("*does not match expected format*");
    }

    [Fact]
    public void ParseSlotId_InvalidTimeFormat_ThrowsArgumentException()
    {
        var act = () => BookingService.ParseSlotId("branch-city-a-2026-05-26-9:0");
        act.Should().Throw<ArgumentException>();
    }

    // ── GenerateBookingRef ────────────────────────────────────────────────────

    [Fact]
    public void GenerateBookingRef_StartsWithBkDash()
    {
        var ref1 = BookingService.GenerateBookingRef();
        ref1.Should().StartWith("bk-");
    }

    [Fact]
    public void GenerateBookingRef_HasLength11()
    {
        var ref1 = BookingService.GenerateBookingRef();
        ref1.Should().HaveLength(11); // "bk-" (3) + 8 chars
    }

    [Fact]
    public void GenerateBookingRef_TwoCallsProduceDifferentValues()
    {
        var ref1 = BookingService.GenerateBookingRef();
        var ref2 = BookingService.GenerateBookingRef();
        ref1.Should().NotBe(ref2);
    }

    [Fact]
    public void GenerateBookingRef_SuffixIsAlphanumeric()
    {
        for (var i = 0; i < 20; i++)
        {
            var bookingRef = BookingService.GenerateBookingRef();
            bookingRef[3..].Should().MatchRegex("^[a-z0-9]{8}$");
        }
    }
}
