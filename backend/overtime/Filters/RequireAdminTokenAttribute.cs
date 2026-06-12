using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Overtime.Service;

namespace Overtime.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireAdminTokenAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue("X-Admin-Token", out var tokenValues) ||
            string.IsNullOrWhiteSpace(tokenValues.FirstOrDefault()) ||
            !headers.TryGetValue("X-Admin-UserId", out var userIdValues) ||
            !Guid.TryParse(userIdValues.FirstOrDefault(), out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tokenService = context.HttpContext.RequestServices
            .GetRequiredService<IAdminTokenService>();

        var token = tokenValues.First()!;
        if (!tokenService.ValidateToken(token, userId))
            context.Result = new UnauthorizedResult();
    }
}
