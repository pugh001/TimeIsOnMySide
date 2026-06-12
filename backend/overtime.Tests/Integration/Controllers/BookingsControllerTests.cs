using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class BookingsControllerTests : IntegrationTestBase
{
    public BookingsControllerTests(IntegrationTestFactory factory) : base(factory) { }

    private object ValidBookingBody(string time = "11:00") => new
    {
        slotId = $"{LocationSlug}-2026-05-27-{time}",
        name = "Jane Doe",
        email = "jane@example.com",
        phone = "0831231234"
    };

    [Fact]
    public async Task PostBooking_ValidRequest_Returns201()
    {
        var response = await Client.PostAsJsonAsync("/api/bookings", ValidBookingBody("11:00"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostBooking_ValidRequest_ReturnsBookingResponse()
    {
        var response = await Client.PostAsJsonAsync("/api/bookings", ValidBookingBody("11:30"));
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty("bookingId", out var bookingId).Should().BeTrue();
        doc.RootElement.TryGetProperty("slotId", out var slotId).Should().BeTrue();
        doc.RootElement.TryGetProperty("startTime", out var startTime).Should().BeTrue();
        doc.RootElement.TryGetProperty("date", out var date).Should().BeTrue();
        doc.RootElement.TryGetProperty("name", out var name).Should().BeTrue();

        bookingId.GetString().Should().StartWith("bk-");
        slotId.GetString().Should().Be($"{LocationSlug}-2026-05-27-11:30");
        startTime.GetString().Should().Be("11:30");
        date.GetString().Should().Be("2026-05-27");
        name.GetString().Should().Be("Jane Doe");
    }

    [Fact]
    public async Task PostBooking_DoubleBook_Returns409()
    {
        // Pre-book all employees at 12:00 using the IDs set during InitializeAsync
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
        db.Bookings.AddRange(
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-conflict1",
                StaffId = Employee1Id,
                SlotDate = new DateOnly(2026, 5, 27), StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(12, 30),
                CustomerName = "A", CustomerEmail = "a@a.com", CustomerPhone = "111", CreatedAt = DateTimeOffset.UtcNow
            },
            new BookingEntity
            {
                Id = Guid.NewGuid(), BookingRef = "bk-conflict2",
                StaffId = Employee2Id,
                SlotDate = new DateOnly(2026, 5, 27), StartTime = new TimeOnly(12, 0), EndTime = new TimeOnly(12, 30),
                CustomerName = "B", CustomerEmail = "b@b.com", CustomerPhone = "222", CreatedAt = DateTimeOffset.UtcNow
            });
        await db.SaveChangesAsync();

        var response = await Client.PostAsJsonAsync("/api/bookings", ValidBookingBody("12:00"));
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task PostBooking_InvalidEmail_Returns422()
    {
        var body = new { slotId = $"{LocationSlug}-2026-05-27-13:00", name = "Jane", email = "not-an-email", phone = "123" };
        var response = await Client.PostAsJsonAsync("/api/bookings", body);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostBooking_MissingName_Returns422()
    {
        var body = new { slotId = $"{LocationSlug}-2026-05-27-13:30", name = "", email = "jane@example.com", phone = "123" };
        var response = await Client.PostAsJsonAsync("/api/bookings", body);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostBooking_MissingPhone_Returns422()
    {
        var body = new { slotId = $"{LocationSlug}-2026-05-27-14:00", name = "Jane", email = "jane@example.com", phone = "" };
        var response = await Client.PostAsJsonAsync("/api/bookings", body);
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostBooking_NoAuthHeaders_Returns201()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/bookings", ValidBookingBody("10:00"));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}
