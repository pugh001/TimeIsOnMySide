using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Overtime.Tests.Integration.Infrastructure;

namespace Overtime.Tests.Integration.Controllers;

public sealed class RateLimitingTests : IntegrationTestBase
{
    public RateLimitingTests(IntegrationTestFactory factory) : base(factory) { }

    [Fact]
    public async Task PostBooking_11thRequestSameClient_Returns429()
    {
        // Send 10 requests — all may succeed or fail for business reasons, but should not be rate-limited
        for (var i = 0; i < 10; i++)
        {
            var body = new
            {
                slotId = $"{LocationSlug}-2026-06-02-09:00",
                name = "Rate Test",
                email = $"rate{i}@test.com",
                phone = "123456789"
            };
            var r = await Client.PostAsJsonAsync("/api/bookings", body);
            r.StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
                because: $"request {i + 1} should not be rate-limited");
        }

        // 11th request must be rate-limited
        var eleventh = await Client.PostAsJsonAsync("/api/bookings", new
        {
            slotId = $"{LocationSlug}-2026-06-02-09:00",
            name = "Rate Test",
            email = "rate11@test.com",
            phone = "123456789"
        });

        eleventh.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
