using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Overtime.Data;
using Overtime.Data.Entities;

namespace Overtime.Tests.Integration.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<IntegrationTestFactory>, IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly IntegrationTestFactory Factory;

    protected Guid LocationId { get; private set; }
    protected Guid Employee1Id { get; private set; }
    protected Guid Employee2Id { get; private set; }

    // Known credentials for the seeded staff user — used by GetStaffTokenAsync
    private string Staff1Username { get; set; } = string.Empty;
    private const string Staff1Password = "StaffP@ss1";

    // Unique per test instance to prevent slug collisions under parallel execution
    protected string LocationSlug { get; } = $"test-branch-{Guid.NewGuid():N}";

    protected IntegrationTestBase(IntegrationTestFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        await db.Database.MigrateAsync();
        await SeedAsync(db);
    }

    public async Task DisposeAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        await CleanupAsync(db, LocationSlug);
        foreach (var slug in _extraLocationSlugs)
            await CleanupAsync(db, slug);
    }

    private async Task SeedAsync(OvertimeDbContext db)
    {
        // Ensure admin user exists with a current valid password hash.
        // The migration removed the old HasData seed, so we re-seed here.
        var adminId = new Guid("00000000-0000-0000-0000-000000000001");
        var adminHasher = new PasswordHasher<UserEntity>();
        var existingAdmin = await db.Users.FirstOrDefaultAsync(u => u.Id == adminId);
        var admin = existingAdmin ?? new UserEntity
        {
            Id = adminId, Username = "admin",
            Role = "admin", FullName = "Administrator",
            FirstName = "Administrator", LastName = string.Empty,
            WorkingTimes = [],
            CreatedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
        };
        System.Diagnostics.Debug.Assert(admin is not null);
        admin.PasswordHash = adminHasher.HashPassword(admin, "admin");
        System.Diagnostics.Debug.Assert(!string.IsNullOrEmpty(admin.PasswordHash));
        if (existingAdmin is null) db.Users.Add(admin);
        else db.Users.Update(admin);
        await db.SaveChangesAsync();

        LocationId = Guid.NewGuid();
        Employee1Id = Guid.NewGuid();
        Employee2Id = Guid.NewGuid();

        // LoginRequestValidator requires: 1-8 letters + exactly 4 digits
        // Derive unique prefix from the guid portion of LocationSlug (after "test-branch-"),
        // keeping only letters to satisfy the validator regex.
        var guidPart = LocationSlug["test-branch-".Length..];
        var prefix = new string(guidPart.Where(char.IsLetter).Take(8).ToArray());
        Staff1Username = prefix + "0001";

        var hasher = new PasswordHasher<UserEntity>();

        var weekdays = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" };

        var location = new LocationEntity
        {
            Id = LocationId,
            Slug = LocationSlug,
            Name = "Test Branch",
            OpeningHours = new LocationOpeningHours
            {
                Monday = Std(), Tuesday = Std(), Wednesday = Std(),
                Thursday = Std(), Friday = Std()
            },
            CreatedAt = DateTimeOffset.UtcNow
        };

        var staff1 = new UserEntity
        {
            Id = Employee1Id, LocationId = LocationId,
            Username = Staff1Username,
            Role = "staff",
            FullName = "Test Staff One",
            FirstName = "Test",
            LastName = "Staff One",
            WorkingTimes = weekdays.Select(Shift).ToList(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        staff1.PasswordHash = hasher.HashPassword(staff1, Staff1Password);

        var staff2 = new UserEntity
        {
            Id = Employee2Id, LocationId = LocationId,
            Username = prefix + "0002",
            PasswordHash = "hash",
            Role = "staff",
            FullName = "Test Staff Two",
            FirstName = "Test",
            LastName = "Staff Two",
            WorkingTimes = weekdays.Select(Shift).ToList(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Add location and users in one SaveChanges to avoid FK deadlocks under parallel test execution
        db.Locations.Add(location);
        db.Users.AddRange(staff1, staff2);
        await db.SaveChangesAsync();
        return;

        static UserWorkingTime Shift(string day) => new() { Day = day, ShiftStart = new TimeOnly(9, 0), ShiftEnd = new TimeOnly(17, 0) };

        static OpeningHours Std() => new() { OpenTime = new TimeOnly(9, 0), CloseTime = new TimeOnly(17, 0) };
    }

    protected async Task<(Guid locationId, string locationSlug)> SeedEarlyOpenLocationAsync(
        TimeOnly openTime, TimeOnly closeTime, TimeOnly shiftStart, TimeOnly shiftEnd)
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();

        var slug = $"early-{Guid.NewGuid():N}";
        var locId = Guid.NewGuid();

        // Each day must be a separate OpeningHours instance — EF owned entities
        // cannot share a single object across multiple navigation properties.
        OpeningHours Hours() => new() { OpenTime = openTime, CloseTime = closeTime };

        var location = new LocationEntity
        {
            Id = locId,
            Slug = slug,
            Name = "Early Open Branch",
            OpeningHours = new LocationOpeningHours
            {
                Monday = Hours(), Tuesday = Hours(), Wednesday = Hours(),
                Thursday = Hours(), Friday = Hours()
            },
            CreatedAt = DateTimeOffset.UtcNow
        };

        var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<UserEntity>();
        var staff = new UserEntity
        {
            Id = Guid.NewGuid(),
            LocationId = locId,
            Username = new string(slug.Where(char.IsLetter).Take(8).ToArray()) + "0001",
            Role = "staff",
            FullName = "Early Staff",
            FirstName = "Early",
            LastName = "Staff",
            WorkingTimes = new[] { "monday", "tuesday", "wednesday", "thursday", "friday" }
                .Select(d => new UserWorkingTime { Day = d, ShiftStart = shiftStart, ShiftEnd = shiftEnd })
                .ToList(),
            CreatedAt = DateTimeOffset.UtcNow
        };
        staff.PasswordHash = hasher.HashPassword(staff, "EarlyP@ss1");

        db.Locations.Add(location);
        db.Users.Add(staff);
        await db.SaveChangesAsync();

        // Track for cleanup
        _extraLocationSlugs.Add(slug);

        return (locId, slug);
    }

    private readonly List<string> _extraLocationSlugs = [];

    protected async Task<(string token, string userId)> GetAdminTokenAsync()
    {
        var resp = await Client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });
        resp.EnsureSuccessStatusCode();
        var doc = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return (doc.RootElement.GetProperty("adminToken").GetString()!,
                doc.RootElement.GetProperty("adminUserId").GetString()!);
    }

    protected async Task<(string token, string userId)> GetStaffTokenAsync()
    {
        var resp = await Client.PostAsJsonAsync("/api/auth/login",
            new { username = Staff1Username, password = Staff1Password });
        resp.EnsureSuccessStatusCode();
        var doc = System.Text.Json.JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("staffToken").GetString()!;
        var userId = doc.RootElement.GetProperty("staffUserId").GetString()!;
        return (token, userId);
    }

    private static async Task CleanupAsync(OvertimeDbContext db, string slug)
    {
        var locationIds = await db.Locations
            .Where(l => l.Slug == slug)
            .Select(l => l.Id)
            .ToListAsync();

        foreach (var locId in locationIds)
        {
            await db.Bookings.Where(b => b.Staff.LocationId == locId).ExecuteDeleteAsync();
            await db.Users.Where(u => u.LocationId == locId && u.Role == "staff").ExecuteDeleteAsync();
        }
        await db.Locations.Where(l => l.Slug == slug).ExecuteDeleteAsync();
    }
}
