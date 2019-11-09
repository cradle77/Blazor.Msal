using Blazor.Msal.Client.AzureAd;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Msal.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorizationCore();
            services.AddScoped<MsalAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
