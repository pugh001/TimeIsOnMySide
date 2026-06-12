using System.Net;
using System.Text.Json;
using FluentValidation;
using Overtime.Common.Exceptions;
using Serilog;

namespace Overtime.DependencyInjection;

public sealed class ApplicationExceptionHandler
{
    private readonly RequestDelegate _next;

    public ApplicationExceptionHandler(RequestDelegate next)
    {
        ArgumentNullException.ThrowIfNull(next);
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (SlotUnavailableException ex)
        {
            await WriteJson(context, HttpStatusCode.Conflict, new { error = ex.Message });
        }
        catch (NotFoundException ex)
        {
            await WriteJson(context, HttpStatusCode.NotFound, new { error = ex.Message });
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors.Select(e => new { field = e.PropertyName, message = e.ErrorMessage }).ToList();
            Log.Warning("Validation failure on {Path}: {FieldCount} error(s) — fields: {Fields}",
                context.Request.Path,
                details.Count,
                string.Join(", ", details.Select(d => d.field)));
            await WriteJson(context, HttpStatusCode.UnprocessableEntity,
                new { error = "Validation failed", details });
        }
        catch (ArgumentException ex)
        {
            Log.Warning(ex, "Bad argument");
            await WriteJson(context, HttpStatusCode.BadRequest, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception");
            await WriteJson(context, HttpStatusCode.InternalServerError,
                new { error = "An unexpected error occurred." });
        }
    }

    private static async Task WriteJson(HttpContext context, HttpStatusCode status, object body)
    {
        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }
}
