using Des.Blazor.Authorization.Msal;
using Des.Blazor.Authorization.Msal.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MsalConfigurationExtensions
    {
        private static IServiceCollection AddBaseServices(this IServiceCollection services)
        {
            services.AddScoped<IMsal, Msal>();
            services.AddScoped<MsalAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });
            services.AddScoped<IAuthenticationManager>(s =>
            {
                return s.GetService<MsalAuthenticationStateProvider>();
            });

            return services;
        }

        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, IMsalConfig config)
        {
            Guard.ThrowIfNull(config, nameof(config));

            services.AddAzureActiveDirectory(sp => Task.FromResult(config));

            return services;
        }

        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, Uri configUri)
        {
            services.AddAzureActiveDirectory(async sp => 
            {
                if (!configUri.IsAbsoluteUri)
                {
                    var manager = sp.GetService<NavigationManager>();
                    var baseUri = new Uri(manager.BaseUri);
                    configUri = new Uri(baseUri, configUri);
                }

                Console.WriteLine("Inside fetch");

                var http = sp.GetService<HttpClient>();

                Console.WriteLine($"Got HttpClient, calling {configUri}");

                var json = await http.GetStringAsync(configUri.ToString());

                Console.WriteLine($"Json received: {json}");

                return JsonSerializer.Deserialize<MsalConfig>(json);
            });

            return services;
        }

        public static IServiceCollection AddAzureActiveDirectory(this IServiceCollection services, Func<IServiceProvider, Task<IMsalConfig>> configurator)
        {
            Guard.ThrowIfNull(configurator, nameof(configurator));

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
