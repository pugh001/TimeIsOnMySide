using Capitec.StrategicProjects.Shared.Hosting;
using RestService.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// If you do not require a database as an example, simply remove the 'builder.ConfigureDatabaseConnections' line.
// If code is enabled below - it will expect the configuration to be present in the local configuration file or externalise config server, otherwise exceptions will be thrown.

builder.LoadConfiguration();
builder.LoadVaultSecrets();
builder.ConfigureFluentbit();
builder.ConfigureDatabaseConnections();
builder.Services.ConfigureRESTControllers();
builder.Services.AddHealthChecks();
builder.Services.ConfigureIdentityProvider(builder);
builder.Services.ConfigureApplicationDependencies();
builder.Services.ReadExampleDomainEndpointConfiguration(builder);
builder.Services.LoadUnleash(builder);

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.ConfigureHealthChecks();
app.UseMiddleware<CorrelationIdLoggingMiddleware>();
app.UseMiddleware<ApplicationExceptionHandler>();
app.ConfigureSwagger();

if (builder.Environment.IsValidEnvironment())
{
    app.Run();
}

//below line is added for Integration tests
namespace RestService
{
    public partial class Program
    {
    }
}
