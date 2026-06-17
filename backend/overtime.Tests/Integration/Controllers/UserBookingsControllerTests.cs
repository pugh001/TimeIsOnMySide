using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class UserBookingsControllerTests : IntegrationTestBase
{
    public UserBookingsControllerTests(IntegrationTestFactory factory) : base(factory) { }

    // ── Auth guard ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserBookings_NoToken_Returns401()
    {
        var response = await Client.GetAsync($"/api/users/{Employee1Id}/bookings");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserBookings_InvalidToken_Returns401()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Employee1Id}/bookings");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", "bad-token");
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", Employee1Id.ToString());
        var response = await Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUserBookings_ValidAdminToken_Returns200()
    {
        var (token, adminUserId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Employee1Id}/bookings");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", adminUserId);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserBookings_NoBookings_ReturnsEmptyArray()
    {
        var (token, adminUserId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Employee1Id}/bookings");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", adminUserId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("bookings").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetUserBookings_WithBookings_ReturnsCorrectShape()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        db.Bookings.Add(new BookingEntity
        {
            Id = Guid.NewGuid(), BookingRef = "bk-admview1",
            StaffId = Employee1Id,
            SlotDate = new DateOnly(2026, 12, 15), StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(11, 30),
            CustomerName = "Admin View Customer", CustomerEmail = "av@test.com", CustomerPhone = "0311111111",
            CreatedAt = DateTimeOffset.UtcNow
        });
        await db.SaveChangesAsync();

        var (token, adminUserId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Employee1Id}/bookings");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", adminUserId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var booking = doc.RootElement.GetProperty("bookings").EnumerateArray().First();

        booking.GetProperty("bookingRef").GetString().Should().Be("bk-admview1");
        booking.GetProperty("date").GetString().Should().Be("2026-12-15");
        booking.GetProperty("startTime").GetString().Should().Be("11:00");
        booking.GetProperty("endTime").GetString().Should().Be("11:30");
        booking.GetProperty("customerName").GetString().Should().Be("Admin View Customer");
    }

    [Fact]
    public async Task GetUserBookings_OnlyReturnsBookingsForRequestedUser()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        db.Bookings.AddRange(
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-emp1-av",
                StaffId = Employee1Id,
                SlotDate = new DateOnly(2026, 12, 16), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30),
                CustomerName = "Emp1 Customer", CustomerEmail = "e1@test.com", CustomerPhone = "0411111111",
                CreatedAt = DateTimeOffset.UtcNow
            },
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-emp2-av",
                StaffId = Employee2Id,
                SlotDate = new DateOnly(2026, 12, 16), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(9, 30),
                CustomerName = "Emp2 Customer", CustomerEmail = "e2@test.com", CustomerPhone = "0422222222",
                CreatedAt = DateTimeOffset.UtcNow
            });
        await db.SaveChangesAsync();

        var (token, adminUserId) = await GetAdminTokenAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/users/{Employee1Id}/bookings");
        request.Headers.TryAddWithoutValidation("X-Admin-Token", token);
        request.Headers.TryAddWithoutValidation("X-Admin-UserId", adminUserId);

        var response = await Client.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var bookings = doc.RootElement.GetProperty("bookings").EnumerateArray().ToList();

        bookings.Should().HaveCount(1);
        bookings[0].GetProperty("customerName").GetString().Should().Be("Emp1 Customer");
    }
}
