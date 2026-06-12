using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class UsersControllerTests : IntegrationTestBase
{
    public UsersControllerTests(IntegrationTestFactory factory) : base(factory) { }

    private object ValidBody() => new
    {
        firstName = "Jane",
        lastName = "Doe",
        password = "secret99",
        locationId = LocationId.ToString(),
        workingTimes = new[]
        {
            new { day = "monday",    shiftStart = "09:00", shiftEnd = "17:00" },
            new { day = "wednesday", shiftStart = "09:00", shiftEnd = "17:00" },
            new { day = "friday",    shiftStart = "09:00", shiftEnd = "17:00" }
        }
    };

    // ── Auth ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task PostUser_NoToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/users", ValidBody());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostUser_WrongToken_Returns401()
    {
        Client.DefaultRequestHeaders.TryAddWithoutValidation("X-Admin-Token", "bad-token");
        Client.DefaultRequestHeaders.TryAddWithoutValidation("X-Admin-UserId", Guid.NewGuid().ToString());

        var response = await Client.PostAsJsonAsync("/api/users", ValidBody());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────

    [Fact]
    public async Task PostUser_ValidRequest_Returns201WithUserIdAndUsername()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
        request.Content = JsonContent.Create(ValidBody());

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.TryGetProperty("userId", out var userIdProp).Should().BeTrue();
        doc.RootElement.TryGetProperty("username", out var usernameProp).Should().BeTrue();
        userIdProp.GetString().Should().NotBeNullOrEmpty();
        usernameProp.GetString().Should().MatchRegex(@"^jane\d{4}$");
    }

    [Fact]
    public async Task PostUser_SecondCallSameFirstName_IncrementsUsername()
    {
        var (token, userId) = await GetAdminTokenAsync();

        async Task<string> CreateUser()
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/users");
            req.Headers.TryAddWithoutValidation("X-Admin-Token", token);
            req.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
            req.Content = JsonContent.Create(new
            {
                firstName = "Unique",
                lastName = "Tester",
                password = "password1",
                locationId = LocationId.ToString(),
                workingTimes = new[] { new { day = "monday", shiftStart = "08:00", shiftEnd = "16:00" } }
            });
            var resp = await Client.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            return doc.RootElement.GetProperty("username").GetString()!;
        }

        var first = await CreateUser();
        var second = await CreateUser();

        first.Should().MatchRegex(@"^unique\d{4}$");
        second.Should().MatchRegex(@"^unique\d{4}$");
        second.Should().NotBe(first);

        var firstNum = int.Parse(first[6..]);
        var secondNum = int.Parse(second[6..]);
        secondNum.Should().Be(firstNum + 1);
    }

    // ── Validation errors ─────────────────────────────────────────────────

    [Fact]
    public async Task PostUser_BadLocationId_Returns422()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
        request.Content = JsonContent.Create(new
        {
            firstName = "Jane",
            lastName = "Doe",
            password = "secret99",
            locationId = Guid.NewGuid().ToString(),
            workingTimes = new[] { new { day = "monday", shiftStart = "09:00", shiftEnd = "17:00" } }
        });

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostUser_PasswordTooShort_Returns422()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
        request.Content = JsonContent.Create(new
        {
            firstName = "Jane",
            lastName = "Doe",
            password = "short",
            locationId = LocationId.ToString(),
            workingTimes = new[] { new { day = "monday", shiftStart = "09:00", shiftEnd = "17:00" } }
        });

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostUser_EmptyWorkingTimes_Returns422()
    {
        var (token, userId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/users");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", userId);
        request.Content = JsonContent.Create(new
        {
            firstName = "Jane",
            lastName = "Doe",
            password = "secret99",
            locationId = LocationId.ToString(),
            workingTimes = Array.Empty<object>()
        });

        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    // ── helpers ───────────────────────────────────────────────────────────

    private async Task<(string token, string userId)> GetAdminTokenAsync()
    {
        var resp = await Client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });
        resp.EnsureSuccessStatusCode();
        var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return (doc.RootElement.GetProperty("adminToken").GetString()!,
                doc.RootElement.GetProperty("adminUserId").GetString()!);
    }
}
