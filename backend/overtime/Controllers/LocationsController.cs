using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Overtime.Filters;
using Overtime.Service;
using Overtime.Service.Models.Locations;

namespace Overtime.Controllers;

[ApiController]
[Route("api/locations")]
[AllowAnonymous]
public sealed class LocationsController : ControllerBase
{
    private readonly ILocationService _service;

    public LocationsController(ILocationService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocations(CancellationToken ct)
    {
        var locations = await _service.GetLocationsAsync(ct);
        return Ok(new { locations });
    }

    [HttpGet("{id:guid}")]
    [RequireAdminToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocation([FromRoute] Guid id, CancellationToken ct)
    {
        var location = await _service.GetByIdAsync(id, ct);
        if (location is null) return NotFound();
        return Ok(location);
    }

    [HttpPost]
    [RequireAdminToken]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _service.CreateAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("{id:guid}/users")]
    [RequireAdminToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationUsers([FromRoute] Guid id, CancellationToken ct)
    {
        var users = await _service.GetUsersForLocationAsync(id, ct);
        return Ok(new { users });
    }
}
