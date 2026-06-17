using Capitec.StrategicProjects.Shared.Hosting;
using RestService.Common.Context;
using RestService.Common.Exceptions;
using RestService.DependencyInjection.Models;
using Serilog;

namespace RestService.DependencyInjection;

public static class Logging
{
    /// <summary>
    /// Configures Serilog to write to FluentD.
    /// Also configures the logger with a Enricher that contains hosting information.
    /// </summary>
    public static void ConfigureFluentbit(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, config) =>
        {
            config.ReadFrom.Configuration(ctx.Configuration);
            config.Enrich.FromLogContext();
            config.WriteTo.Console(outputTemplate:
                                   "{Timestamp:HH:mm} [{Level}] {Properties}: {Message}{NewLine}{Exception}");

            if (!builder.Environment.IsLocal())
            {
                ApplicationHostingContext.ReadHostingConfiguration(builder.Configuration);
                config.Enrich.With(new HostingContextEnricher());
                var fluentBitOptions =
                    builder.Configuration.GetRequiredSection("FluentBit").Get<FluentBitOptions>() ??
                    throw new
                        ConfigurationException($"Configuration for key 'FluentBit' is missing");

                config.WriteTo.Fluentd(fluentBitOptions.Host, fluentBitOptions.Port,
                                       fluentBitOptions.Tag,
                                       fluentBitOptions.RestrictedToMinimumLevel);
            }
        });
    }
}