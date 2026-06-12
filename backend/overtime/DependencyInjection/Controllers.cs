using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Overtime.DependencyInjection;

public static class Controllers
{
    public static void ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(x =>
                x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(options =>
            {
                // Return 422 for validation failures instead of the default 400
                options.InvalidModelStateResponseFactory = context =>
                    new UnprocessableEntityObjectResult(context.ModelState);
            });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<Program>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}
