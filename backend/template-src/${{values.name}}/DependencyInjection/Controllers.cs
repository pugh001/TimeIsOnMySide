using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;

namespace RestService.DependencyInjection;

/// <summary>
/// Setup the controllers
/// </summary>
public static class Controllers
{
    public static void ConfigureRESTControllers(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddControllers().AddFluentValidation()
                .AddJsonOptions(x =>
                                    x.JsonSerializerOptions.Converters
                                     .Add(new JsonStringEnumConverter()));

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddHealthChecks();
    }
}