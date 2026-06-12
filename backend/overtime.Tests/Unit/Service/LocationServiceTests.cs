using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Locations;

namespace Overtime.Tests.Unit.Service;

public sealed class LocationServiceTests : IDisposable
{
    private readonly OvertimeDbContext _db;
    private readonly LocationService _service;

    public LocationServiceTests()
    {
        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new OvertimeDbContext(options);
        _service = new LocationService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetLocationsAsync_EmptyDb_ReturnsEmptyList()
    {
        var result = await _service.GetLocationsAsync();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLocationsAsync_WithLocations_ReturnsMappedList()
    {
        _db.Locations.AddRange(
            new LocationEntity { Id = Guid.NewGuid(), Slug = "branch-a", Name = "Branch A", CreatedAt = DateTimeOffset.UtcNow },
            new LocationEntity { Id = Guid.NewGuid(), Slug = "branch-b", Name = "Branch B", CreatedAt = DateTimeOffset.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _service.GetLocationsAsync();

        result.Should().HaveCount(2);
        result.Should().Contain(l => l.Slug == "branch-a" && l.Name == "Branch A");
        result.Should().Contain(l => l.Slug == "branch-b" && l.Name == "Branch B");
    }

    [Fact]
    public async Task GetLocationsAsync_IdIsUuid_SlugMatchesEntitySlug()
    {
        var entityId = Guid.NewGuid();
        _db.Locations.Add(new LocationEntity
        {
            Id = entityId, Slug = "my-slug", Name = "My Location", CreatedAt = DateTimeOffset.UtcNow
        });
        await _db.SaveChangesAsync();

        var result = await _service.GetLocationsAsync();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(entityId);
        result[0].Slug.Should().Be("my-slug");
    }

    [Fact]
    public async Task GetLocationsAsync_ReturnsOrderedByName()
    {
        _db.Locations.AddRange(
            new LocationEntity { Id = Guid.NewGuid(), Slug = "z-branch", Name = "Z Branch", CreatedAt = DateTimeOffset.UtcNow },
            new LocationEntity { Id = Guid.NewGuid(), Slug = "a-branch", Name = "A Branch", CreatedAt = DateTimeOffset.UtcNow });
        await _db.SaveChangesAsync();

        var result = await _service.GetLocationsAsync();

        result[0].Name.Should().Be("A Branch");
        result[1].Name.Should().Be("Z Branch");
    }

    [Fact]
    public async Task CreateAsync_HappyPath_ReturnsSlugAndId()
    {
        var request = new CreateLocationRequest
        {
            Name = "My New Branch",
            Address = "1 Main Street"
        };

        var result = await _service.CreateAsync(request);

        result.Slug.Should().Be("my-new-branch");
        result.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateAsync_PersistsToDatabase()
    {
        var request = new CreateLocationRequest
        {
            Name = "Persisted Branch",
            Address = "99 Storage Lane"
        };

        var result = await _service.CreateAsync(request);

        var saved = await _db.Locations.FindAsync(result.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Persisted Branch");
        saved.Address.Should().Be("99 Storage Lane");
        saved.Slug.Should().Be("persisted-branch");
    }

    // ── Slug uniqueness ───────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_DuplicateName_SecondLocationGetsSuffix2()
    {
        var request = new CreateLocationRequest { Name = "Same Name" };
        var first = await _service.CreateAsync(request);
        var second = await _service.CreateAsync(request);

        first.Slug.Should().Be("same-name");
        second.Slug.Should().Be("same-name-2");
    }

    [Fact]
    public async Task CreateAsync_TripleDuplicateName_ThirdLocationGetsSuffix3()
    {
        var request = new CreateLocationRequest { Name = "Triple Name" };
        await _service.CreateAsync(request);
        await _service.CreateAsync(request);
        var third = await _service.CreateAsync(request);

        third.Slug.Should().Be("triple-name-3");
    }

    [Fact]
    public async Task CreateAsync_SlugSequenceExhausted_ThrowsInvalidOperationException()
    {
        // Seed slugs "exhausted", "exhausted-2" ... "exhausted-100" to fill all 100 slots.
        _db.Locations.Add(new LocationEntity { Id = Guid.NewGuid(), Slug = "exhausted", Name = "E0", CreatedAt = DateTimeOffset.UtcNow });
        for (var i = 2; i <= 100; i++)
            _db.Locations.Add(new LocationEntity { Id = Guid.NewGuid(), Slug = $"exhausted-{i}", Name = $"E{i}", CreatedAt = DateTimeOffset.UtcNow });
        await _db.SaveChangesAsync();

        var act = async () => await _service.CreateAsync(new CreateLocationRequest { Name = "Exhausted" });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Slug sequence exhausted*");
    }

    [Fact]
    public async Task CreateAsync_WithOpeningHours_PersistsOpeningHours()
    {
        var request = new CreateLocationRequest
        {
            Name = "Hours Branch",
            Address = "7 Clock Road",
            OpeningHours = new Dictionary<string, OpeningHoursDto?>
            {
                ["monday"] = new OpeningHoursDto("08:00", "17:00"),
                ["tuesday"] = new OpeningHoursDto("09:00", "18:00")
            }
        };

        var result = await _service.CreateAsync(request);

        var saved = await _db.Locations.FindAsync(result.Id);
        saved!.OpeningHours.Should().NotBeNull();
        saved.OpeningHours!.Monday.Should().NotBeNull();
        saved.OpeningHours.Monday!.OpenTime.Should().Be(new TimeOnly(8, 0));
        saved.OpeningHours.Monday.CloseTime.Should().Be(new TimeOnly(17, 0));
    }
}
