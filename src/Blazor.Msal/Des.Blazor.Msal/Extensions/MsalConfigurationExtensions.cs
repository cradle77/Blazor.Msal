using Des.Blazor.Msal.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MsalConfigurationExtensions
    {
        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services)
        {
            services.AddScoped<MsalAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });

            return services;
        }
    }
}
