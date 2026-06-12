using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Overtime.Common.Context;
using Overtime.Data.Entities;
using Overtime.Service;
using Overtime.Service.Models.Email;

namespace Overtime.DependencyInjection;

public static class ApplicationDependencies
{
    public static IServiceCollection ConfigureApplicationDependencies(
        this IServiceCollection services, IConfiguration config)
    {
        services.Configure<EmailOptions>(config.GetSection("Email"));
        services.AddHttpContextAccessor();
        services.AddScoped<IApplicationContext, ApplicationContext>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ISlotService, SlotService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<EmailQueue>();
        services.AddHostedService<EmailDispatcherService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddSingleton<IPasswordHasher<UserEntity>, PasswordHasher<UserEntity>>();
        services.AddSingleton<IAdminTokenService>(sp =>
        {
            var secret = Environment.GetEnvironmentVariable("ADMIN_TOKEN_SECRET")
                ?? "dev-admin-token-secret-change-in-prod!";
            return new AdminTokenService(secret);
        });
        return services;
    }
}
