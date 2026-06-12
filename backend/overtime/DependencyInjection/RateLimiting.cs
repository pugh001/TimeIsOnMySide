using System.Threading.RateLimiting;

namespace Overtime.DependencyInjection;

public static class RateLimiting
{
    public const string BookingsPolicy = "bookings";
    public const string LoginPolicy = "login";

    public static IServiceCollection ConfigureRateLimiting(this IServiceCollection services, IConfiguration config)
    {
        if (config.GetValue<bool>("RateLimiting:Disabled"))
        {
            services.AddRateLimiter(_ => { });
            return services;
        }
        return services.ConfigureRateLimiting();
    }

    private static IServiceCollection ConfigureRateLimiting(this IServiceCollection services)
    {

        services.AddRateLimiter(options =>
        {
            options.AddPolicy(BookingsPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.AddPolicy(LoginPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}
