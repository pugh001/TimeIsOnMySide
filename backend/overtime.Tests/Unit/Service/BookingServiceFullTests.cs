using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Overtime.Common.Exceptions;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Bookings;

namespace Overtime.Tests.Unit.Service;

public sealed class BookingServiceFullTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly BookingService _service;

    private static readonly Guid LocationId = Guid.NewGuid();
    private static readonly Guid Staff1Id = Guid.NewGuid();
    private static readonly Guid Staff2Id = Guid.NewGuid();

    public BookingServiceFullTests()
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
            Slug = "branch-city-a",
            Name = "City Branch A",
            CreatedAt = DateTimeOffset.UtcNow
        });
        static UserWorkingTime Shift(string day) => new() { Day = day, ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) };
        var weekdays = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" };

        _db.Users.AddRange(
            new UserEntity
            {
                Id = Staff1Id,
                Username = "staff0001",
                PasswordHash = "hash",
                Role = "staff",
                FullName = "Staff One",
                FirstName = "Staff",
                LastName = "One",
                LocationId = LocationId,
                WorkingTimes = weekdays.Select(Shift).ToList(),
                CreatedAt = DateTimeOffset.UtcNow
            },
            new UserEntity
            {
                Id = Staff2Id,
                Username = "staff0002",
                PasswordHash = "hash",
                Role = "staff",
                FullName = "Staff Two",
                FirstName = "Staff",
                LastName = "Two",
                LocationId = LocationId,
                WorkingTimes = weekdays.Select(Shift).ToList(),
                CreatedAt = DateTimeOffset.UtcNow
            });
        _db.SaveChanges();
    }

    private static CreateBookingRequest ValidRequest(string slotId = "branch-city-a-2026-05-27-10:00") => new()
    {
        SlotId = slotId,
        Name = "Jane Doe",
        Email = "jane@example.com",
        Phone = "+1234567890"
    };

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_AvailableSlot_ReturnsResponse()
    {
        var response = await _service.CreateBookingAsync(ValidRequest());

        response.Should().NotBeNull();
        response.SlotId.Should().Be("branch-city-a-2026-05-27-10:00");
        response.StartTime.Should().Be("10:00");
        response.BookingId.Should().StartWith("bk-");
        response.Date.Should().Be("2026-05-27");
        response.Name.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task CreateBookingAsync_CreatesBookingInDb()
    {
        await _service.CreateBookingAsync(ValidRequest());

        var booking = await _db.Bookings.FirstOrDefaultAsync();
        booking.Should().NotBeNull();
        booking!.CustomerName.Should().Be("Jane Doe");
        booking.CustomerEmail.Should().Be("jane@example.com");
        booking.SlotDate.Should().Be(new DateOnly(2026, 5, 27));
        booking.StartTime.Should().Be(new TimeOnly(10, 0));
        booking.EndTime.Should().Be(new TimeOnly(10, 30));
    }

    [Fact]
    public async Task CreateBookingAsync_AssignsAvailableEmployee()
    {
        await _service.CreateBookingAsync(ValidRequest());

        var booking = await _db.Bookings.FirstAsync();
        var validStaffIds = new[] { Staff1Id, Staff2Id };
        validStaffIds.Should().Contain(booking.StaffId);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNotes_SavesNotes()
    {
        var req = ValidRequest();
        req.Notes = "Bring documents";
        await _service.CreateBookingAsync(req);

        var booking = await _db.Bookings.FirstAsync();
        booking.Notes.Should().Be("Bring documents");
    }

    // ── Slot unavailable ──────────────────────────────────────────────────────

    // Note: UseInMemoryDatabase does not enforce unique constraints, so the uq_employee_slot
    // DB-level path (race-condition duplicate insert) cannot be tested here.
    // That code path is covered by the integration tests which run against real PostgreSQL.
    [Fact]
    public async Task CreateBookingAsync_AllEmployeesBooked_ThrowsSlotUnavailableException()
    {
        // Book staff 1
        _db.Bookings.Add(new BookingEntity
        {
            Id = Guid.NewGuid(), BookingRef = "bk-existing1",
            StaffId = Staff1Id,
            SlotDate = new DateOnly(2026, 5, 27), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10, 30),
            CustomerName = "A", CustomerEmail = "a@a.com", CustomerPhone = "111", CreatedAt = DateTimeOffset.UtcNow
        });
        // Book staff 2
        _db.Bookings.Add(new BookingEntity
        {
            Id = Guid.NewGuid(), BookingRef = "bk-existing2",
            StaffId = Staff2Id,
            SlotDate = new DateOnly(2026, 5, 27), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10, 30),
            CustomerName = "B", CustomerEmail = "b@b.com", CustomerPhone = "222", CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        var act = () => _service.CreateBookingAsync(ValidRequest());
        await act.Should().ThrowAsync<SlotUnavailableException>();
    }

    // ── Location not found ────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_UnknownLocation_ThrowsNotFoundException()
    {
        var req = ValidRequest("unknown-location-2026-05-27-10:00");
        var act = () => _service.CreateBookingAsync(req);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*unknown-location*");
    }

    // ── Null guard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateBookingAsync_NullRequest_ThrowsArgumentNullException()
    {
        var act = () => _service.CreateBookingAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}
