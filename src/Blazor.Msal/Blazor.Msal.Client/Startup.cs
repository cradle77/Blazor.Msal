using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Blazor.Msal.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorizationCore();
            services.AddAzureActiveDirectory(async sp => 
            {
                var http = sp.GetService<HttpClient>();

                var config = await http.GetJsonAsync<ClientConfig>($"/config/appsettings.json?{DateTime.Now.Ticks}");

                return config;
            });
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
