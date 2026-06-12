using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class AuthControllerTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidAdminCredentials_Returns200WithBody()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("employeeName", out var name).Should().BeTrue();
        doc.RootElement.TryGetProperty("role", out var role).Should().BeTrue();
        name.GetString().Should().Be("Administrator");
        role.GetString().Should().Be("admin");
    }

    [Fact]
    public async Task Login_AdminCredentials_ReturnsAdminTokenAndUserId()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "admin" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("adminToken", out var token).Should().BeTrue();
        doc.RootElement.TryGetProperty("adminUserId", out var userId).Should().BeTrue();
        token.GetString().Should().NotBeNullOrEmpty();
        userId.GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "wrongpassword" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        doc.RootElement.TryGetProperty("error", out _).Should().BeTrue();
    }

    [Fact]
    public async Task Login_UnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "nobody0001", password = "admin" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyUsername_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "", password = "admin" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_EmptyPassword_Returns422()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Logout_Returns200WithMessage()
    {
        var response = await _client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("message").GetString().Should().Be("Logged out");
    }
}
