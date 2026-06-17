using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Unleash;
using ArgumentException = System.ArgumentException;

namespace RestService.DependencyInjection
{
    /// <summary>
    /// Loads unleash configuration from the appsettings, creates a singleton unleash client to be used in the application.
    /// </summary>
    public static class UnleashIntegration
    {
        public static void LoadUnleash(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var unleashConfig =
                builder.Configuration.GetRequiredSection(UnleashConfig.SectionName).Get<UnleashConfig>() ??
                throw new
                    ArgumentException(
                        $"Configuration for key '{UnleashConfig.SectionName}' is missing");
            
            if (String.IsNullOrEmpty(unleashConfig.Project)) 
                throw new ArgumentException($"Unleash Project is missing");
            if (String.IsNullOrEmpty(unleashConfig.AppName)) 
                throw new ArgumentException($"Unleash AppName is missing");
            if (String.IsNullOrEmpty(unleashConfig.InstanceTag)) 
                throw new ArgumentException($"Unleash InstanceTag is missing");
            if (String.IsNullOrEmpty(unleashConfig.UnleashApi)) 
                throw new ArgumentException($"Unleash UnleashApi is missing");
            if (String.IsNullOrEmpty(unleashConfig.Token)) 
                throw new ArgumentException($"Unleash Token is missing");
            if (unleashConfig.FetchTogglesInterval == 0) 
                throw new ArgumentException($"Unleash FetchTogglesInterval is not set");

            var unleashSettings = new UnleashSettings
            {
                ProjectId = unleashConfig.Project,
                AppName = unleashConfig.AppName,
                InstanceTag = unleashConfig.InstanceTag,
                UnleashApi = new Uri(unleashConfig.UnleashApi),
                CustomHttpHeaders = new Dictionary<string, string> { { "Authorization", unleashConfig.Token } },
                FetchTogglesInterval = new TimeSpan(0,0,unleashConfig.FetchTogglesInterval)
            };
            
            var unleashClient = new DefaultUnleash(unleashSettings);
            
            services.AddSingleton<IUnleash>(unleashClient);
        }
    }
}