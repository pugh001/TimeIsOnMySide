using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Overtime.Service;

namespace Overtime.Controllers;

[ApiController]
[Route("api/slots")]
[AllowAnonymous]
public sealed class SlotsController : ControllerBase
{
    private readonly ISlotService _service;

    public SlotsController(ISlotService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSlots(
        [FromQuery] string? date,
        [FromQuery] string? locationId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(date))
            return BadRequest(new { error = "Query parameter 'date' is required." });

        if (string.IsNullOrWhiteSpace(locationId))
            return BadRequest(new { error = "Query parameter 'locationId' is required." });

        var slots = await _service.GetSlotsAsync(date, locationId, ct);
        return Ok(new { slots });
    }
}
