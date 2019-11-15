using Des.Blazor.Authorization.Msal;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MsalConfigurationExtensions
    {
        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, Func<IServiceProvider, Task<IMsalConfig>> configurator)
        {
            services.AddScoped<IConfigProvider<IMsalConfig>>(sp => 
            {
                return new DelegateConfigProvider<IMsalConfig>(sp, configurator);
            });

            services.AddScoped<MsalAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });

            return services;
        }
    }
}
