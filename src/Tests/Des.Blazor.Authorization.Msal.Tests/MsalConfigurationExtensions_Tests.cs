using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using System;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Des.Blazor.Authorization.Msal.Tests
{
    public class MsalConfigurationExtensions_Tests
    {
        [Fact]
        public void AddAzureActiveDirectory_WithNullServicesAndConfig_ThrowsArgumentNullException()
        {
            IServiceCollection services = null;
            IMsalConfig config = null;

            Assert.Throws<ArgumentNullException>(() => services.AddAzureActiveDirectory(config));
        }

        [Fact]
        public void AddAzureActiveDirectory_WithNullServicesAndConfigurator_ThrowsArgumentNullException()
        {
            IServiceCollection services = null;
            Func<IServiceProvider, Task<IMsalConfig>> configurator = null;

            Assert.Throws<ArgumentNullException>(() => services.AddAzureActiveDirectory(configurator));
        }

        [Fact]
        public void AddAzureActiveDirectory_WithNullConfig_ThrowsArgumentNullException()
        {
            var servicesMock = new Mock<IServiceCollection>();
            IServiceCollection services = servicesMock.Object;
            IMsalConfig config = null;

            Assert.Throws<ArgumentNullException>(() => services.AddAzureActiveDirectory(config));
        }

        [Fact]
        public void AddAzureActiveDirectory_WithNullConfigurator_ThrowsArgumentNullException()
        {
            var servicesMock = new Mock<IServiceCollection>();
            IServiceCollection services = servicesMock.Object;
            Func<IServiceProvider, Task<IMsalConfig>> configurator = null;

            Assert.Throws<ArgumentNullException>(() => services.AddAzureActiveDirectory(configurator));
        }

        [Fact]
        public void AddAzureActiveDirectory_WithConfigurator_RegistersServices()
        {
            var services = new ServiceCollection();
            Func<IServiceProvider, Task<IMsalConfig>> configurator = 
                (sp) => Task.FromResult((IMsalConfig)null);

            services.AddAzureActiveDirectory(configurator);

            Assert.Contains(services, s => s.ServiceType == typeof(AuthenticationStateProvider));
            Assert.Contains(services, s => s.ServiceType == typeof(IAuthenticationManager));
            Assert.Contains(services, s => s.ServiceType == typeof(IConfigProvider<IMsalConfig>));
            Assert.Contains(services, s => s.ServiceType == typeof(IMsal));
        }

        [Fact]
        public void AddAzureActiveDirectory_RegistersSameInstances_ForStateProvider()
        {
            var services = new ServiceCollection();

            var jsRuntime = new Mock<IJSRuntime>();
            services.AddTransient(sp => jsRuntime.Object);
            var httpClient = new Mock<HttpClient>();
            services.AddTransient(sp => httpClient.Object);
            var navigationManager = new Mock<NavigationManager>();
            services.AddTransient(sp => navigationManager.Object);

            Func<IServiceProvider, Task<IMsalConfig>> configurator =
                (sp) => Task.FromResult((IMsalConfig)null);

            services.AddAzureActiveDirectory(configurator);

            var provider = services.BuildServiceProvider();

            var authenticationManager = provider.GetService<IAuthenticationManager>();
            var authenticationState = provider.GetService<AuthenticationStateProvider>();

            Assert.Same(authenticationManager, authenticationState);            
        }
    }
}
