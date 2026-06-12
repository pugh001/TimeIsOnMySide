using HealthChecks.NpgSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Overtime.Data;
using Overtime.Data.Entities;
using Overtime.DependencyInjection;
using Overtime.Service.Models.Email;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLogging();
builder.ConfigureDatabase();
builder.Services.ConfigureControllers();
builder.Services.ConfigureApplicationDependencies(builder.Configuration);
var isLocalDev = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("local");
if (!isLocalDev)
    builder.Services.AddSingleton<IValidateOptions<EmailOptions>, EmailOptionsValidator>();
builder.Services.ConfigureRateLimiting(builder.Configuration);
var healthConnStr = builder.Configuration.GetConnectionString("Default")
    ?? throw new Overtime.Common.Exceptions.ConfigurationException("ConnectionStrings:Default is missing from configuration.");
builder.Services.AddHealthChecks().AddNpgSql(new NpgSqlHealthCheckOptions(healthConnStr),
    "postgres");
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

if (args.Contains("-m"))
{
    Console.WriteLine("db migration started");
    Log.Information("Database migration started");
    var migrationApp = builder.Build();
    using var scope = migrationApp.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<OvertimeDbContext>();
    await db.Database.MigrateAsync();
    Log.Information("Database migrated");
    await SeedAdminAsync(scope.ServiceProvider);
    Log.Information("Seed complete");
    return;
}

static async Task SeedAdminAsync(IServiceProvider services)
{
    var db = services.GetRequiredService<OvertimeDbContext>();
    if (await db.Users.AnyAsync(u => u.Role == "admin")) return;
    var hasher = services.GetRequiredService<IPasswordHasher<UserEntity>>();
    var admin = new UserEntity
    {
        Id = Guid.NewGuid(),
        Username = "admin",
        Role = "admin",
        FullName = "Admin",
        FirstName = "Admin",
        LastName = "User",
        CreatedAt = DateTimeOffset.UtcNow,
    };
    admin.PasswordHash = hasher.HashPassword(admin, "Admin1234!");
    db.Users.Add(admin);
    await db.SaveChangesAsync();
}

var app = builder.Build();

app.UseRouting();
app.UseCors();
app.UseRateLimiter();
app.UseMiddleware<CorrelationIdLoggingMiddleware>();
app.UseMiddleware<ApplicationExceptionHandler>();
app.MapControllers();
app.ConfigureHealthChecks();
app.ConfigureSwagger();

app.Run();

namespace Overtime
{
    public partial class Program { }
}
