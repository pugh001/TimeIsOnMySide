using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using NSubstitute;
using Overtime.Data;
using Overtime.Service;

namespace Overtime.Tests.Integration.Infrastructure;

public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("local");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext registration with one pointing at the test DB
            // (same local PG, separate database to isolate integration tests)
            services.RemoveAll<DbContextOptions<OvertimeDbContext>>();
            services.RemoveAll<OvertimeDbContext>();

            var dataSource = new NpgsqlDataSourceBuilder(
                "Host=localhost;Port=5432;Database=overtime_test;Username=db_app_user;Password=p0stgr3s@!23")
                .EnableDynamicJson()
                .Build();

            services.AddDbContext<OvertimeDbContext>(options =>
                options.UseNpgsql(dataSource));

            // Replace notification service with a no-op mock so integration tests
            // never attempt real SMTP connections
            services.RemoveAll<INotificationService>();
            services.AddSingleton<INotificationService>(Substitute.For<INotificationService>());

        });
    }
}
