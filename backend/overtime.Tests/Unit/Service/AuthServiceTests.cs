using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Auth;

namespace Overtime.Tests.Unit.Service;

public sealed class AuthServiceTests
{
    private static OvertimeDbContext BuildInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new OvertimeDbContext(options);
    }

    private static AdminTokenService BuildTokenService()
        => new("test-secret-32-chars-long-enough!");

    private static async Task<(AuthService service, OvertimeDbContext db)> CreateAsync(
        string dbName, string plainPassword, string role = "staff")
    {
        var db = BuildInMemoryDb(dbName);
        var hasher = new PasswordHasher<UserEntity>();
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            Role = role,
            FullName = "Test User",
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = hasher.HashPassword(user, plainPassword);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        var service = new AuthService(db, hasher, BuildTokenService());
        return (service, db);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsLoginResponse()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_ValidCredentials_ReturnsLoginResponse), "P@ssword1");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.EmployeeName.Should().Be("Test User");
        result.Role.Should().Be("staff");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsNull()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_WrongPassword_ReturnsNull), "P@ssword1");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "wrong" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UnknownUser_ReturnsNull()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_UnknownUser_ReturnsNull), "P@ssword1");

        var result = await service.LoginAsync(new LoginRequest { Username = "nobody", Password = "P@ssword1" });

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_AdminUser_ReturnsAdminToken()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_AdminUser_ReturnsAdminToken), "P@ssword1", "admin");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.AdminToken.Should().NotBeNullOrEmpty();
        result.AdminUserId.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_StaffUser_AdminTokenIsNull()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_StaffUser_AdminTokenIsNull), "P@ssword1", "staff");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.AdminToken.Should().BeNull();
        result.AdminUserId.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_StaffUser_ReturnsStaffToken()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_StaffUser_ReturnsStaffToken), "P@ssword1", "staff");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.StaffToken.Should().NotBeNullOrEmpty();
        result.StaffUserId.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_AdminUser_StaffTokenIsNull()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_AdminUser_StaffTokenIsNull), "P@ssword1", "admin");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.StaffToken.Should().BeNull();
        result.StaffUserId.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_UnknownRole_AllTokensNull()
    {
        var (service, _) = await CreateAsync(nameof(LoginAsync_UnknownRole_AllTokensNull), "P@ssword1", "unknown-role");

        var result = await service.LoginAsync(new LoginRequest { Username = "testuser", Password = "P@ssword1" });

        result.Should().NotBeNull();
        result!.AdminToken.Should().BeNull();
        result.AdminUserId.Should().BeNull();
        result.StaffToken.Should().BeNull();
        result.StaffUserId.Should().BeNull();
    }
}
