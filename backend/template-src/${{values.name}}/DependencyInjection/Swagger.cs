using Capitec.StrategicProjects.Shared.Hosting;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace RestService.DependencyInjection;

/// <summary>
/// Configures Swagger
/// </summary>
public static class Swagger
{
    public static WebApplication ConfigureSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        if (!app.Environment.IsLocal())
        {
            app.UseSwaggerUI(settings =>
            {
                // Disable the "Try it out" functionality.
                settings.SupportedSubmitMethods(Array.Empty<SubmitMethod>());
            });
        }

        return app;
    }
}