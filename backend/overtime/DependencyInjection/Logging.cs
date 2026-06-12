using Serilog;

namespace Overtime.DependencyInjection;

public static class Logging
{
    public static void ConfigureLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, config) =>
        {
            config.ReadFrom.Configuration(ctx.Configuration);
            config.Enrich.FromLogContext();
            config.WriteTo.Console(outputTemplate:
                "{Timestamp:HH:mm} [{Level}] {Properties}: {Message}{NewLine}{Exception}");
        });
    }
}
