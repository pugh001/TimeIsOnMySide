using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Overtime.Data;
using Overtime.Service;

namespace Overtime.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireStaffTokenAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var headers = context.HttpContext.Request.Headers;

        if (!headers.TryGetValue("X-Staff-Token", out var tokenValues) ||
            string.IsNullOrWhiteSpace(tokenValues.FirstOrDefault()) ||
            !headers.TryGetValue("X-Staff-UserId", out var userIdValues) ||
            !Guid.TryParse(userIdValues.FirstOrDefault(), out var userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tokenService = context.HttpContext.RequestServices
            .GetRequiredService<IAdminTokenService>();

        if (!tokenService.ValidateToken(tokenValues.First()!, userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var db = context.HttpContext.RequestServices.GetRequiredService<OvertimeDbContext>();
        var role = db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Role)
            .FirstOrDefault();

        if (role != "staff")
            context.Result = new UnauthorizedResult();
    }
}
