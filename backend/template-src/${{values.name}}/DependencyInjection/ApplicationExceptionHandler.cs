using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using Serilog;

namespace RestService.DependencyInjection;

/// <summary>
/// Centralized Exception handler that translates exceptions to HTTP status codes.
/// Below is an example. Edit as needed
/// </summary>
public class ApplicationExceptionHandler
{
    private readonly RequestDelegate _next;

    public ApplicationExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }

        catch (ValidationException ex)
        {
            await BadRequest(context, ex);
        }
        
        catch (Exception ex)
        {
            InternalServerError(context, ex);
        }
    }

    private static void InternalServerError(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        Log.Error(ex.Message, ex.StackTrace);
    }

    private static async Task BadRequest(HttpContext context, Exception ex)
    {
        Object errorResponse = null;
        
        Log.Warning(ex.Message, ex.StackTrace);
        errorResponse = new { Message = ex.Message };
        
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        context.Response.ContentType = "application/json";
        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(jsonResponse);
    }
}