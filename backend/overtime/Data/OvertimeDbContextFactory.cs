using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Overtime.Data;

public sealed class OvertimeDbContextFactory : IDesignTimeDbContextFactory<OvertimeDbContext>
{
    public OvertimeDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=overtime;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<OvertimeDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new OvertimeDbContext(options);
    }
}
