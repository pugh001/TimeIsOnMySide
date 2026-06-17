using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestService.Service;
using RestService.Service.Models.CreateExample;
using RestService.Service.Models.DeleteExample;
using RestService.Service.Models.RetrieveFromDatabaseExample;

namespace RestService.Controllers;

/// <summary>
/// Example REST controller
/// </summary>
[Route($"api/Example")]
public class RestExampleController : ControllerBase
{
    private readonly IServiceExample _service;

    public RestExampleController(IServiceExample service)
    {
        _service = service;
    }

    /// <summary>
    /// GET
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.FailedDependency)]
    [ProducesResponseType(typeof(RetrieveFromDatabaseResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RetrieveFromDatabaseResponse>> Get(
        RetrieveFromDatabaseRequest fromDatabaseRequest, CancellationToken cancellationToken)
    {
        return await _service.RetrieveFromDatabase(fromDatabaseRequest);
    }

    /// <summary>
    /// POST
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.FailedDependency)]
    [ProducesResponseType(typeof(CreateResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Create([FromBody] CreateRequest request,
        CancellationToken cancellationToken)
    {
        await _service.Create(request);

        return Ok();
    }

    /// <summary>
    /// POST
    /// </summary>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
    [ProducesResponseType((int)HttpStatusCode.Forbidden)]
    [ProducesResponseType((int)HttpStatusCode.BadRequest)]
    [ProducesResponseType((int)HttpStatusCode.Conflict)]
    [ProducesResponseType((int)HttpStatusCode.FailedDependency)]
    [ProducesResponseType(typeof(CreateResponse), StatusCodes.Status200OK)]
    public async Task<bool> Delete(DeleteRequest request, CancellationToken cancellationToken)
    {
        return await _service.Delete(request);
    }
}