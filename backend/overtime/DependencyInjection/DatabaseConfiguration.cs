using Microsoft.EntityFrameworkCore;
using Npgsql;
using Overtime.Common.Exceptions;
using Overtime.Data;

namespace Overtime.DependencyInjection;

public static class DatabaseConfiguration
{
    public static void ConfigureDatabase(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Default")
            ?? throw new ConfigurationException("ConnectionStrings:Default is missing from configuration.");

        var dataSource = new NpgsqlDataSourceBuilder(connectionString)
            .EnableDynamicJson()
            .Build();

        builder.Services.AddDbContext<OvertimeDbContext>(options =>
            options.UseNpgsql(dataSource, npgsql =>
                npgsql.EnableRetryOnFailure(maxRetryCount: 3)));
    }
}
