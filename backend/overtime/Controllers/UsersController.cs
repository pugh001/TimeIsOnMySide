using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Overtime.Filters;
using Overtime.Service;
using Overtime.Service.Models.Users;

namespace Overtime.Controllers;

[ApiController]
[Route("api/users")]
[AllowAnonymous]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IBookingService _bookingService;

    public UsersController(IUserService userService, IBookingService bookingService)
    {
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(bookingService);
        _userService = userService;
        _bookingService = bookingService;
    }

    [HttpPost]
    [RequireAdminToken]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _userService.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{userId:guid}/bookings")]
    [RequireAdminToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserBookings([FromRoute] Guid userId, CancellationToken ct)
    {
        var bookings = await _bookingService.GetBookingsByUserIdAsync(userId, ct);
        return Ok(new { bookings });
    }
}
