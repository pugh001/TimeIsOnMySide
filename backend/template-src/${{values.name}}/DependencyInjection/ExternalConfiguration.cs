using Capitec.StrategicProjects.Shared.ConfigServer;
using Capitec.StrategicProjects.Shared.Hosting;

namespace RestService.DependencyInjection;

public static class ExternalConfiguration
{
    /// <summary>
    /// Loads the configuration from the appsettings json file or from the externalised configuration server.
    /// </summary>
    public static void LoadConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{builder.Environment}.json", optional: true,
                            reloadOnChange: true).AddEnvironmentVariables();

        if (builder.Environment.IsLocal())
        {
            return;
        }

        var settings = new ConfigServerClientSettings
        {
            Name = builder.Environment.ApplicationName,
            Label = builder.Environment.EnvironmentName
        };

        builder.Configuration.AddConfigServer(settings);
    }
}