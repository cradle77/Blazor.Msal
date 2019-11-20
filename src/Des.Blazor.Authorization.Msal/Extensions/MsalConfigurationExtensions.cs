using Des.Blazor.Authorization.Msal;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MsalConfigurationExtensions
    {
        private static IServiceCollection AddBaseServices(this IServiceCollection services)
        {
            services.AddScoped<MsalAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });

            return services;
        }

        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, IMsalConfig config)
        {
            services.AddAzureActiveDirectory(sp => Task.FromResult(config));

            return services;
        }

        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, Func<IServiceProvider, Task<IMsalConfig>> configurator)
        {
            services
                .AddBaseServices()
                .AddScoped<IConfigProvider<IMsalConfig>>(sp => 
                {
                    return new DelegateConfigProvider<IMsalConfig>(sp, configurator);
                });

            return services;
        }
    }
}
