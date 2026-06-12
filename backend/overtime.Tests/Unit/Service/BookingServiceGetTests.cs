using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;

namespace Overtime.Tests.Unit.Service;

public sealed class BookingServiceGetTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly BookingService _service;

    private static readonly Guid LocationId = Guid.NewGuid();
    private static readonly Guid Staff1Id = Guid.NewGuid();
    private static readonly Guid Staff2Id = Guid.NewGuid();

    public BookingServiceGetTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new OvertimeDbContext(options);
        _service = new BookingService(_db, new EmailQueue(), Substitute.For<ILogger<BookingService>>());
        SeedBaseData();
    }

    public void Dispose() => _db.Dispose();

    private void SeedBaseData()
    {
        _db.Locations.Add(new LocationEntity
        {
            Id = LocationId,
            Slug = "test-branch",
            Name = "Test Branch",
            CreatedAt = DateTimeOffset.UtcNow
        });
        static UserWorkingTime Shift(string day) => new() { Day = day, ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) };
        var weekdays = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" };

        _db.Users.AddRange(
            new UserEntity
            {
                Id = Staff1Id, Username = "staff0001", PasswordHash = "hash",
                Role = "staff", FullName = "Staff One", FirstName = "Staff", LastName = "One",
                LocationId = LocationId,
                WorkingTimes = weekdays.Select(Shift).ToList(),
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserEntity
            {
                Id = Staff2Id, Username = "staff0002", PasswordHash = "hash",
                Role = "staff", FullName = "Staff Two", FirstName = "Staff", LastName = "Two",
                LocationId = LocationId,
                WorkingTimes = weekdays.Select(Shift).ToList(),
                CreatedAt = DateTimeOffset.UtcNow
            });
        _db.SaveChanges();
    }

    private BookingEntity MakeBooking(Guid staffId, DateOnly date, TimeOnly start, string bookingRef, string customerName)
        => new()
        {
            Id = Guid.NewGuid(),
            BookingRef = bookingRef,
            StaffId = staffId,
            SlotDate = date,
            StartTime = start,
            EndTime = start.AddMinutes(30),
            CustomerName = customerName,
            CustomerEmail = "test@test.com",
            CustomerPhone = "0123456789",
            CreatedAt = DateTimeOffset.UtcNow
        };

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookingsAsync_StaffWithBookings_ReturnsTheirBookings()
    {
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(9, 0), "bk-aaa00001", "Alice"));
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(10, 0), "bk-aaa00002", "Bob"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Should().HaveCount(2);
        result.Select(b => b.CustomerName).Should().BeEquivalentTo(["Alice", "Bob"]);
    }

    [Fact]
    public async Task GetBookingsAsync_ReturnsCorrectFields()
    {
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(9, 0), "bk-fields01", "Carol"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        var b = result.Single();
        b.BookingRef.Should().Be("bk-fields01");
        b.Date.Should().Be("2026-12-10");
        b.StartTime.Should().Be("09:00");
        b.EndTime.Should().Be("09:30");
        b.CustomerName.Should().Be("Carol");
    }

    [Fact]
    public async Task GetBookingsAsync_OrderedByDateThenStartTime()
    {
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 11), new TimeOnly(9, 0),  "bk-ord01", "C"));
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(14, 0), "bk-ord02", "B"));
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(9, 0),  "bk-ord03", "A"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Select(b => b.CustomerName).Should().ContainInOrder("A", "B", "C");
    }

    [Fact]
    public async Task GetBookingsAsync_OnlyReturnsBookingsForRequestedStaff()
    {
        _db.Bookings.Add(MakeBooking(Staff1Id, new DateOnly(2026, 12, 10), new TimeOnly(9, 0),  "bk-s1", "For Staff1"));
        _db.Bookings.Add(MakeBooking(Staff2Id, new DateOnly(2026, 12, 10), new TimeOnly(10, 0), "bk-s2", "For Staff2"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Should().HaveCount(1);
        result.Single().CustomerName.Should().Be("For Staff1");
    }

    [Fact]
    public async Task GetBookingsAsync_NoBookings_ReturnsEmpty()
    {
        var result = await _service.GetBookingsAsync(Staff1Id);
        result.Should().BeEmpty();
    }

    // ── Past filtering ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookingsAsync_ExcludesPastBookings_ReturnsOnlyUpcoming()
    {
        var past = new DateOnly(2026, 1, 1);
        var future = new DateOnly(2026, 12, 1);
        _db.Bookings.Add(MakeBooking(Staff1Id, past,   new TimeOnly(9, 0), "bk-past01", "Past Customer"));
        _db.Bookings.Add(MakeBooking(Staff1Id, future, new TimeOnly(9, 0), "bk-fut01",  "Future Customer"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Should().HaveCount(1);
        result.Single().CustomerName.Should().Be("Future Customer");
    }

    [Fact]
    public async Task GetBookingsAsync_OnlyFutureBookings_ReturnsFutureOnly()
    {
        var past   = new DateOnly(2025, 1, 1);
        var future = new DateOnly(2027, 1, 1);
        _db.Bookings.Add(MakeBooking(Staff1Id, past,   new TimeOnly(9, 0),  "bk-all01", "Historical"));
        _db.Bookings.Add(MakeBooking(Staff1Id, future, new TimeOnly(10, 0), "bk-all02", "Upcoming"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Should().HaveCount(1);
        result.Single().CustomerName.Should().Be("Upcoming");
    }

    [Fact]
    public async Task GetBookingsAsync_TodayBooking_IsIncluded()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        _db.Bookings.Add(MakeBooking(Staff1Id, today, new TimeOnly(9, 0), "bk-today1", "Today Customer"));
        await _db.SaveChangesAsync();

        var result = await _service.GetBookingsAsync(Staff1Id);

        result.Should().HaveCount(1);
        result.Single().CustomerName.Should().Be("Today Customer");
    }

    // ── Guard conditions ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookingsAsync_EmptyStaffId_ThrowsArgumentException()
    {
        var act = () => _service.GetBookingsAsync(Guid.Empty);
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
