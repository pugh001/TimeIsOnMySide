using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Users;

namespace Overtime.Tests.Unit.Service;

public sealed class UserServiceTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly UserService _service;
    private readonly Guid _locationId = Guid.NewGuid();

    public UserServiceTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new OvertimeDbContext(options);
        _db.Locations.Add(new LocationEntity
        {
            Id = _locationId,
            Slug = "test-loc",
            Name = "Test Location",
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();

        _service = new UserService(_db, new PasswordHasher<UserEntity>());
    }

    public void Dispose() => _db.Dispose();

    private CreateUserRequest ValidRequest(string firstName = "Jane") => new()
    {
        FirstName = firstName,
        LastName = "Doe",
        Password = "secret99",
        LocationId = _locationId.ToString(),
        WorkingTimes =
        [
            new UserWorkingTimeDto("monday", "09:00", "17:00"),
            new UserWorkingTimeDto("wednesday", "09:00", "17:00")
        ]
    };

    [Fact]
    public async Task CreateAsync_GeneratesUsername_FromFirstName()
    {
        var result = await _service.CreateAsync(ValidRequest("Jane"));

        result.Username.Should().Be("jane0001");
    }

    [Fact]
    public async Task CreateAsync_GeneratesUsername_IncrementsSequence()
    {
        await _service.CreateAsync(ValidRequest("Jane"));
        var result = await _service.CreateAsync(ValidRequest("Jane"));

        result.Username.Should().Be("jane0002");
    }

    [Fact]
    public async Task CreateAsync_HashesPassword()
    {
        var result = await _service.CreateAsync(ValidRequest());

        var saved = await _db.Users.SingleAsync(u => u.Id == Guid.Parse(result.UserId));
        saved.PasswordHash.Should().NotBe("secret99");
        saved.PasswordHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task CreateAsync_SetsRole_ToStaff()
    {
        var result = await _service.CreateAsync(ValidRequest());

        var saved = await _db.Users.SingleAsync(u => u.Id == Guid.Parse(result.UserId));
        saved.Role.Should().Be("staff");
    }

    [Fact]
    public async Task CreateAsync_PersistsAllFields()
    {
        var result = await _service.CreateAsync(new CreateUserRequest
        {
            FirstName = "Alice",
            LastName = "Smith",
            Password = "password1",
            LocationId = _locationId.ToString(),
            WorkingTimes =
            [
                new UserWorkingTimeDto("monday", "08:00", "16:00"),
                new UserWorkingTimeDto("tuesday", "08:00", "16:00"),
                new UserWorkingTimeDto("friday", "08:00", "16:00")
            ]
        });

        var saved = await _db.Users.SingleAsync(u => u.Id == Guid.Parse(result.UserId));
        saved.FirstName.Should().Be("Alice");
        saved.LastName.Should().Be("Smith");
        saved.FullName.Should().Be("Alice Smith");
        saved.LocationId.Should().Be(_locationId);
        saved.WorkingTimes.Should().HaveCount(3);
        saved.WorkingTimes.Select(w => w.Day).Should().BeEquivalentTo(["monday", "tuesday", "friday"]);
        saved.WorkingTimes.Should().AllSatisfy(w =>
        {
            w.ShiftStart.Should().Be(new TimeOnly(8, 0));
            w.ShiftEnd.Should().Be(new TimeOnly(16, 0));
        });
        saved.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CreateAsync_InvalidLocationId_ThrowsValidationException()
    {
        var request = ValidRequest() with { LocationId = Guid.NewGuid().ToString() };

        var act = async () => await _service.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task CreateAsync_NonGuidLocationId_ThrowsValidationException()
    {
        var request = ValidRequest() with { LocationId = "not-a-guid" };

        var act = async () => await _service.CreateAsync(request);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
