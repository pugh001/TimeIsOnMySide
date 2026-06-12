using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;

namespace Overtime.Tests.Unit.Service;

public sealed class LocationServiceGetUsersTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly LocationService _service;

    public LocationServiceGetUsersTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new OvertimeDbContext(options);
        _service = new LocationService(_db);
    }

    public void Dispose() => _db.Dispose();

    private static UserEntity MakeStaff(Guid locationId, string fullName, string username, UserWorkingTime[]? times = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            LocationId = locationId,
            Username = username,
            PasswordHash = "hash",
            Role = "staff",
            FullName = fullName,
            FirstName = fullName.Split(' ')[0],
            LastName = fullName.Split(' ').ElementAtOrDefault(1) ?? string.Empty,
            WorkingTimes = times ?? [new UserWorkingTime { Day = "monday", ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) }],
            CreatedAt = DateTimeOffset.UtcNow
        };

    [Fact]
    public async Task GetUsersForLocationAsync_UnknownLocation_ThrowsNotFoundException()
    {
        var act = async () => await _service.GetUsersForLocationAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<Common.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task GetUsersForLocationAsync_NoStaff_ReturnsEmpty()
    {
        var locationId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity { Id = locationId, Slug = "loc-a", Name = "Loc A", CreatedAt = DateTimeOffset.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _service.GetUsersForLocationAsync(locationId);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUsersForLocationAsync_WithStaff_ReturnsMappedWorkingTimes()
    {
        var locationId = Guid.NewGuid();
        var staffId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity { Id = locationId, Slug = "loc-b", Name = "Loc B", CreatedAt = DateTimeOffset.UtcNow });
        _db.Users.Add(new UserEntity
        {
            Id = staffId, LocationId = locationId,
            Username = "jane0001", PasswordHash = "hash",
            Role = "staff", FullName = "Jane Doe",
            FirstName = "Jane", LastName = "Doe",
            WorkingTimes =
            [
                new UserWorkingTime { Day = "monday", ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) },
                new UserWorkingTime { Day = "tuesday", ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) }
            ],
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetUsersForLocationAsync(locationId);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(staffId);
        result[0].FullName.Should().Be("Jane Doe");
        result[0].WorkingTimes.Should().HaveCount(2);
        result[0].WorkingTimes.Select(w => w.Day).Should().BeEquivalentTo(["monday", "tuesday"]);
        result[0].WorkingTimes.Should().AllSatisfy(w =>
        {
            w.ShiftStart.Should().Be("09:00");
            w.ShiftEnd.Should().Be("17:00");
        });
    }

    [Fact]
    public async Task GetUsersForLocationAsync_AdminUserIgnored_OnlyStaffReturned()
    {
        var locationId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity { Id = locationId, Slug = "loc-c", Name = "Loc C", CreatedAt = DateTimeOffset.UtcNow });
        _db.Users.AddRange(
            new UserEntity
            {
                Id = Guid.NewGuid(), LocationId = locationId,
                Username = "admin0001", PasswordHash = "hash",
                Role = "admin", FullName = "Admin User",
                FirstName = "Admin", LastName = "User",
                WorkingTimes = [],
                CreatedAt = DateTimeOffset.UtcNow
            },
            MakeStaff(locationId, "Staff User", "staff0001"));
        await _db.SaveChangesAsync();

        var result = await _service.GetUsersForLocationAsync(locationId);

        result.Should().HaveCount(1);
        result[0].FullName.Should().Be("Staff User");
    }

    [Fact]
    public async Task GetUsersForLocationAsync_OrderedByFullName()
    {
        var locationId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity { Id = locationId, Slug = "loc-d", Name = "Loc D", CreatedAt = DateTimeOffset.UtcNow });
        _db.Users.AddRange(
            MakeStaff(locationId, "Zoe Z", "zoe0001"),
            MakeStaff(locationId, "Adam A", "adam0001"));
        await _db.SaveChangesAsync();

        var result = await _service.GetUsersForLocationAsync(locationId);

        result[0].FullName.Should().Be("Adam A");
        result[1].FullName.Should().Be("Zoe Z");
    }

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsLocationWithOpeningHours()
    {
        var locationId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity
        {
            Id = locationId, Slug = "loc-e", Name = "Loc E",
            OpeningHours = new LocationOpeningHours
            {
                Monday = new OpeningHours { OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0) }
            },
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetByIdAsync(locationId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(locationId);
        result.OpeningHours.Should().NotBeNull();
        result.OpeningHours!["monday"]!.OpenTime.Should().Be("09:00");
        result.OpeningHours["monday"]!.CloseTime.Should().Be("17:00");
        result.OpeningHours.Should().ContainKey("tuesday");
        result.OpeningHours["tuesday"].Should().BeNull();
    }
}
