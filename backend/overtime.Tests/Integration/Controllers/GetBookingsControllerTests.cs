using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class GetBookingsControllerTests : IntegrationTestBase
{
    public GetBookingsControllerTests(IntegrationTestFactory factory) : base(factory) { }

    // ── Auth guard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookings_NoToken_Returns401()
    {
        var response = await Client.GetAsync("/api/bookings");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBookings_InvalidToken_Returns401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", "bad-token");
        request.Headers.TryAddWithoutValidation("X-Staff-UserId", Employee1Id.ToString());
        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBookings_MissingUserId_Returns401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", "some-token");
        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetBookings_ValidToken_Returns200()
    {
        var (token, userId) = await GetStaffTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", token);
        request.Headers.TryAddWithoutValidation("X-Staff-UserId", userId);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetBookings_NoBookings_ReturnsEmptyArray()
    {
        var (token, userId) = await GetStaffTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", token);
        request.Headers.TryAddWithoutValidation("X-Staff-UserId", userId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("bookings").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetBookings_WithBookings_ReturnsCorrectShape()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        db.Bookings.Add(new BookingEntity
        {
            Id = Guid.NewGuid(), BookingRef = "bk-intgtest1",
            StaffId = Employee1Id,
            SlotDate = new DateOnly(2026, 12, 10), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(10, 30),
            CustomerName = "Integration Customer", CustomerEmail = "i@test.com", CustomerPhone = "0111111111",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var (token, userId) = await GetStaffTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", token);
        request.Headers.TryAddWithoutValidation("X-Staff-UserId", userId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var booking = doc.RootElement.GetProperty("bookings").EnumerateArray().First();

        booking.GetProperty("bookingRef").GetString().Should().Be("bk-intgtest1");
        booking.GetProperty("date").GetString().Should().Be("2026-12-10");
        booking.GetProperty("startTime").GetString().Should().Be("10:00");
        booking.GetProperty("endTime").GetString().Should().Be("10:30");
        booking.GetProperty("customerName").GetString().Should().Be("Integration Customer");
    }

    [Fact]
    public async Task GetBookings_OnlyReturnsOwnBookings()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        db.Bookings.AddRange(
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-mine01",
                StaffId = Employee1Id,
                SlotDate = new DateOnly(2026, 12, 10), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30),
                CustomerName = "Mine", CustomerEmail = "m@test.com", CustomerPhone = "0111111111",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-others01",
                StaffId = Employee2Id,
                SlotDate = new DateOnly(2026, 12, 10), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30),
                CustomerName = "Theirs", CustomerEmail = "t@test.com", CustomerPhone = "0222222222",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await db.SaveChangesAsync();

        var (token, userId) = await GetStaffTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/bookings");
        request.Headers.TryAddWithoutValidation("X-Staff-Token", token);
        request.Headers.TryAddWithoutValidation("X-Staff-UserId", userId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var bookings = doc.RootElement.GetProperty("bookings").EnumerateArray().ToList();

        bookings.Should().HaveCount(1);
        bookings[0].GetProperty("customerName").GetString().Should().Be("Mine");
    }
}
