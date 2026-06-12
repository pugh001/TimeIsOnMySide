using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class LocationsControllerTests : IntegrationTestBase
{
    public LocationsControllerTests(IntegrationTestFactory factory) : base(factory) { }

    // ── GET /api/locations ────────────────────────────────────────────────

    [Fact]
    public async Task GetLocations_Returns200()
    {
        var response = await Client.GetAsync("/api/locations");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLocations_ReturnsLocationsArray()
    {
        var response = await Client.GetAsync("/api/locations");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("locations", out var locations).Should().BeTrue();
        locations.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetLocations_ContainsSeededLocation()
    {
        var response = await Client.GetAsync("/api/locations");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var locations = doc.RootElement.GetProperty("locations").EnumerateArray().ToList();
        locations.Should().Contain(l =>
            l.GetProperty("id").GetGuid() == LocationId &&
            l.GetProperty("name").GetString() == "Test Branch");
    }

    [Fact]
    public async Task GetLocations_EachLocationHasIdSlugAndName()
    {
        var response = await Client.GetAsync("/api/locations");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        foreach (var loc in doc.RootElement.GetProperty("locations").EnumerateArray())
        {
            loc.TryGetProperty("id", out var id).Should().BeTrue();
            loc.TryGetProperty("slug", out _).Should().BeTrue();
            loc.TryGetProperty("name", out _).Should().BeTrue();
            Guid.TryParse(id.GetString(), out _).Should().BeTrue("id must be a UUID");
        }
    }

    [Fact]
    public async Task GetLocations_SeededLocationSlugMatchesExpected()
    {
        var response = await Client.GetAsync("/api/locations");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        var locations = doc.RootElement.GetProperty("locations").EnumerateArray().ToList();
        locations.Should().Contain(l =>
            l.GetProperty("slug").GetString() == LocationSlug &&
            l.GetProperty("name").GetString() == "Test Branch");
    }

    [Fact]
    public async Task GetLocations_EachLocationHasOpeningHoursProperty()
    {
        var response = await Client.GetAsync("/api/locations");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        foreach (var loc in doc.RootElement.GetProperty("locations").EnumerateArray())
        {
            loc.TryGetProperty("openingHours", out _).Should().BeTrue();
        }
    }

    // ── POST /api/locations ───────────────────────────────────────────────

    [Fact]
    public async Task PostLocation_NoToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/locations", new
        {
            name = "New Branch",
            address = "1 Test Street"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLocation_WrongToken_Returns401()
    {
        Client.DefaultRequestHeaders.TryAddWithoutValidation("X-Admin-Token", "bad-token");
        Client.DefaultRequestHeaders.TryAddWithoutValidation("X-Admin-UserId", Guid.NewGuid().ToString());

        var response = await Client.PostAsJsonAsync("/api/locations", new
        {
            name = "New Branch",
            address = "1 Test Street"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostLocation_ValidToken_Returns201WithSlugAndId()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/locations");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
        request.Content = JsonContent.Create(new
        {
            name = $"Created Branch {Guid.NewGuid():N}",
            address = "42 Admin Road",
            openingHours = new
            {
                monday = new { openTime = "08:00", closeTime = "17:00" },
                tuesday = new { openTime = "08:00", closeTime = "17:00" }
            }
        });

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("slug", out var slug).Should().BeTrue();
        doc.RootElement.TryGetProperty("id", out var id).Should().BeTrue();
        slug.GetString().Should().NotBeNullOrEmpty();
        id.GetString().Should().NotBeNullOrEmpty();
    }

    // ── GET /api/locations/{id}/users ─────────────────────────────────────

    [Fact]
    public async Task GetLocationUsers_NoToken_Returns401()
    {
        var response = await Client.GetAsync($"/api/locations/{LocationId}/users");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetLocationUsers_ValidToken_Returns200()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/locations/{LocationId}/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetLocationUsers_Returns2SeededStaffMembers()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/locations/{LocationId}/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);

        var response = await Client.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("users", out var users).Should().BeTrue();
        users.EnumerateArray().Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLocationUsers_EachUserHasExpectedShape()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/locations/{LocationId}/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);

        var response = await Client.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        foreach (var user in doc.RootElement.GetProperty("users").EnumerateArray())
        {
            user.TryGetProperty("id", out _).Should().BeTrue();
            user.TryGetProperty("fullName", out _).Should().BeTrue();
            user.TryGetProperty("workingTimes", out var workingTimes).Should().BeTrue();
            workingTimes.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }

    [Fact]
    public async Task GetLocationUsers_UnknownLocationId_Returns404()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/locations/{Guid.NewGuid()}/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private async Task<(string token, string userId)> GetAdminTokenAsync()
    {
        var resp = await Client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });
        resp.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var token = doc.RootElement.GetProperty("adminToken").GetString()!;
        var userId = doc.RootElement.GetProperty("adminUserId").GetString()!;
        return (token, userId);
    }
}
