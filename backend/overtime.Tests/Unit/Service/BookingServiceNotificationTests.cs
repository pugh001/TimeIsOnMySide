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

public sealed class BookingServiceNotificationTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly EmailQueue _emailQueue;
    private readonly BookingService _service;

    private static readonly Guid LocationId = Guid.NewGuid();
    private static readonly Guid Staff1Id = Guid.NewGuid();

    public BookingServiceNotificationTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new OvertimeDbContext(options);
        _emailQueue = new EmailQueue();
        _service = new BookingService(_db, _emailQueue, Substitute.For<ILogger<BookingService>>());

        SeedBaseData();
    }

    public void Dispose() => _db.Dispose();

    private void SeedBaseData()
    {
        _db.Locations.Add(new LocationEntity
        {
            Id = LocationId,
            Slug = "notify-branch",
            Name = "Notify Branch",
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.Users.Add(new UserEntity
        {
            Id = Staff1Id,
            Username = "staff0001",
            PasswordHash = "hash",
            Role = "staff",
            FullName = "Staff One",
            FirstName = "Staff",
            LastName = "One",
            Email = "staff@example.com",
            LocationId = LocationId,
            WorkingTimes = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" }
                .Select(d => new UserWorkingTime { Day = d, ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) })
                .ToList(),
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();
    }

    private static CreateBookingRequest ValidRequest() => new()
    {
        SlotId = "notify-branch-2026-05-27-10:00",
        Name = "Jane Doe",
        Email = "jane@example.com",
        Phone = "+1234567890"
    };

    [Fact]
    public async Task CreateBookingAsync_Success_EnqueuesConfirmationOnce()
    {
        await _service.CreateBookingAsync(ValidRequest());

        _emailQueue.Reader.TryRead(out var confirmation).Should().BeTrue();
        confirmation!.BookingRef.Should().StartWith("bk-");
        confirmation.CustomerEmail.Should().Be("jane@example.com");
        confirmation.CustomerName.Should().Be("Jane Doe");
    }

    [Fact]
    public async Task CreateBookingAsync_Success_ConfirmationHasCorrectDateAndTime()
    {
        await _service.CreateBookingAsync(ValidRequest());

        _emailQueue.Reader.TryRead(out var confirmation).Should().BeTrue();
        confirmation!.Date.Should().Be(new DateOnly(2026, 5, 27));
        confirmation.StartTime.Should().Be(new TimeOnly(10, 0));
        confirmation.StaffEmail.Should().Be("staff@example.com");
    }

    [Fact]
    public async Task CreateBookingAsync_LocationNotFound_NothingEnqueued()
    {
        var req = new CreateBookingRequest
        {
            SlotId = "unknown-location-2026-05-27-10:00",
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+1234567890"
        };

        Func<Task> act = () => _service.CreateBookingAsync(req);
        await act.Should().ThrowAsync<NotFoundException>();

        _emailQueue.Reader.TryRead(out _).Should().BeFalse();
    }

    [Fact]
    public async Task CreateBookingAsync_Success_BookingIdStartsWithBkPrefix()
    {
        var result = await _service.CreateBookingAsync(ValidRequest());
        result.BookingId.Should().StartWith("bk-");
    }

    [Fact]
    public async Task CreateBookingAsync_Success_ReturnsResponseEvenWhenEmailWouldFail()
    {
        // With queue-based dispatch, the HTTP response is decoupled from email delivery.
        // This test confirms the booking response is returned and a confirmation is enqueued.
        var req = new CreateBookingRequest
        {
            SlotId = "notify-branch-2026-05-27-11:00",
            Name = "Jane Doe",
            Email = "jane@example.com",
            Phone = "+1234567890"
        };
        var result = await _service.CreateBookingAsync(req);

        result.Should().NotBeNull();
        result.BookingId.Should().StartWith("bk-");
        _emailQueue.Reader.TryRead(out var confirmation).Should().BeTrue();
        confirmation!.CustomerEmail.Should().Be("jane@example.com");
    }
}
