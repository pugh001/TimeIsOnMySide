using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Overtime.DependencyInjection;
using Overtime.Filters;
using Overtime.Service;
using Overtime.Service.Models.Bookings;

namespace Overtime.Controllers;

[ApiController]
[Route("api/bookings")]
[AllowAnonymous]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _service;

    public BookingsController(IBookingService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpGet]
    [RequireStaffToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookings(CancellationToken ct)
    {
        if (!Guid.TryParse(Request.Headers["X-Staff-UserId"].FirstOrDefault(), out var staffUserId))
            return Unauthorized();

        var bookings = await _service.GetBookingsAsync(staffUserId, ct);
        return Ok(new { bookings });
    }

    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(RateLimiting.BookingsPolicy)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct)
    {
        var response = await _service.CreateBookingAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, response);
    }
}
