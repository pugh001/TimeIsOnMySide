using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Users;

namespace Overtime.Tests.Unit.Service;

public sealed class UserServiceEmailTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly UserService _service;
    private readonly Guid _locationId = Guid.NewGuid();

    public UserServiceEmailTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new OvertimeDbContext(options);
        _service = new UserService(_db, new PasswordHasher<UserEntity>());

        _db.Locations.Add(new LocationEntity
        {
            Id = _locationId,
            Slug = "email-test-branch",
            Name = "Email Test Branch",
            CreatedAt = DateTimeOffset.UtcNow
        });
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private CreateUserRequest ValidRequest(string? email = null) => new()
    {
        FirstName = "Alice",
        LastName = "Smith",
        Password = "Pass1234!",
        LocationId = _locationId.ToString(),
        Email = email,
        WorkingTimes = [new UserWorkingTimeDto("monday", "09:00", "17:00")]
    };

    [Fact]
    public async Task CreateAsync_WithEmail_PersistsEmail()
    {
        await _service.CreateAsync(ValidRequest("staff@example.com"));

        var entity = await _db.Users.FirstAsync(u => u.Role == "staff");
        entity.Email.Should().Be("staff@example.com");
    }

    [Fact]
    public async Task CreateAsync_WithoutEmail_EmailIsNull()
    {
        await _service.CreateAsync(ValidRequest(null));

        var entity = await _db.Users.FirstAsync(u => u.Role == "staff");
        entity.Email.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_WithEmail_ResponseContainsEmail()
    {
        var response = await _service.CreateAsync(ValidRequest("staff@example.com"));

        response.Email.Should().Be("staff@example.com");
    }
}
