using Amazon.RDS.Util;
using Capitec.StrategicProjects.Shared.Hosting;
using Npgsql;
using RestService.Common.Exceptions;
using RestService.DataAccess;

namespace RestService.DependencyInjection;

/// <summary>
/// Configures the read and write database connections for the application
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Reads the connecetion strings and builds the two types of connections.
    /// A token is periodically provided to the connection to allow if to access the RDS instances in AWS.
    /// </summary>]
    public static void ConfigureDatabaseConnections(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        Connection.ReadWriteConnectionBuilder =
            new
                NpgsqlDataSourceBuilder(configuration
                                            .GetValue<
                                                string>("Database:ReadWrite:ConnectionString") ??
                                        throw new
                                            ConfigurationException($"Configuration for key 'Database:ReadWrite:ConnectionString' is missing"));

        Connection.ReadOnlyConnectionBuilder =
            new
                NpgsqlDataSourceBuilder(configuration
                                            .GetValue<
                                                string>("Database:ReadOnly:ConnectionString") ??
                                        throw new
                                            ConfigurationException($"Configuration for key 'Database:ReadOnly:ConnectionString' is missing"));

        if (!builder.Environment.IsLocal())
        {
            Connection.ReadWriteConnectionBuilder
                      .UsePeriodicPasswordProvider((settings, cancellationToken) => ValueTask.FromResult(RDSAuthTokenGenerator.GenerateAuthToken(settings.Host, settings.Port, settings.Username)),
                                                   TimeSpan
                                                       .FromMinutes(14), // Interval for refreshing the token
                                                   TimeSpan
                                                       .FromSeconds(5)); // Interval for retrying after a refresh failure

            Connection.ReadOnlyConnectionBuilder
                      .UsePeriodicPasswordProvider((settings, cancellationToken) => ValueTask.FromResult(RDSAuthTokenGenerator.GenerateAuthToken(settings.Host, settings.Port, settings.Username)),
                                                   TimeSpan
                                                       .FromMinutes(14), // Interval for refreshing the token
                                                   TimeSpan
                                                       .FromSeconds(5)); // Interval for retrying after a refresh failure
        }
    }
}