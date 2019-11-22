using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System;
using System.Linq;
using System.Net.Cache;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
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

        [Fact]
        public async Task AddAzureActiveDirectory_WithUri_FetchesConfigFromUri()
        {
            var services = new ServiceCollection();

            var jsRuntime = new Mock<IJSRuntime>();
            services.AddTransient(sp => jsRuntime.Object);

            var config = new TestConfig();
            var json =
@"{
  ""Authority"":  ""https://myauthority.com/"",
  ""ClientId"":  ""12345""
}";

            var httpResponse = new HttpResponseMessage()
            {
                Content = new StringContent(json)
            };

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task.FromResult(httpResponse));

            services.AddTransient(sp => new HttpClient(httpMessageHandler.Object));

            var navigationManager = new TestNavigationManager();
            services.AddTransient<NavigationManager>(sp => navigationManager);

            services.AddAzureActiveDirectory(new Uri("config/config.json", UriKind.Relative));

            var provider = services.BuildServiceProvider();
            var configurator = provider.GetService<IConfigProvider<IMsalConfig>>();

            var result = await configurator.GetConfigurationAsync();

            Assert.Equal(config.ClientId, result.ClientId);
            Assert.Equal(config.Authority, result.Authority);
            Assert.Equal(config.LoginMode, result.LoginMode);
        }
    }
}
