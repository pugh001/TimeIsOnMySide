using System.Net;
using System.Text.Json;
using FluentAssertions;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class SlotsControllerTests : IntegrationTestBase
{
    public SlotsControllerTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task GetSlots_ValidWeekday_Returns200With16Slots()
    {
        var response = await Client.GetAsync($"/api/slots?date=2026-05-27&locationId={LocationId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var slots = doc.RootElement.GetProperty("slots").EnumerateArray().ToList();
        slots.Should().HaveCount(16);
    }

    [Fact]
    public async Task GetSlots_Weekend_Returns200WithEmptyArray()
    {
        var response = await Client.GetAsync($"/api/slots?date=2026-05-23&locationId={LocationId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var slots = doc.RootElement.GetProperty("slots").EnumerateArray().ToList();
        slots.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSlots_MissingDate_Returns400()
    {
        var response = await Client.GetAsync($"/api/slots?locationId={LocationId}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSlots_MissingLocationId_Returns400()
    {
        var response = await Client.GetAsync("/api/slots?date=2026-05-27");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSlots_BadDateFormat_Returns400()
    {
        var response = await Client.GetAsync($"/api/slots?date=27-05-2026&locationId={LocationId}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSlots_SlotsHaveCorrectShape()
    {
        var response = await Client.GetAsync($"/api/slots?date=2026-05-27&locationId={LocationId}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var slot = doc.RootElement.GetProperty("slots").EnumerateArray().First();

        slot.TryGetProperty("id", out _).Should().BeTrue();
        slot.TryGetProperty("date", out _).Should().BeTrue();
        slot.TryGetProperty("startTime", out _).Should().BeTrue();
        slot.TryGetProperty("endTime", out _).Should().BeTrue();
        slot.TryGetProperty("status", out _).Should().BeTrue();
        slot.TryGetProperty("locationId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetSlots_FirstSlotIs0900_LastIs1630()
    {
        var response = await Client.GetAsync($"/api/slots?date=2026-05-27&locationId={LocationId}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var slots = doc.RootElement.GetProperty("slots").EnumerateArray().ToList();

        slots[0].GetProperty("startTime").GetString().Should().Be("09:00");
        slots[15].GetProperty("startTime").GetString().Should().Be("16:30");
    }

    [Fact]
    public async Task GetSlots_SlotIdMatchesFormat()
    {
        var response = await Client.GetAsync($"/api/slots?date=2026-05-27&locationId={LocationId}");
        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        var firstSlot = doc.RootElement.GetProperty("slots").EnumerateArray().First();

        firstSlot.GetProperty("id").GetString()
            .Should().Be($"{LocationSlug}-2026-05-27-09:00");
    }
}
