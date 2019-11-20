using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
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
        public void AddAzureActiveDirectory_WithNullFunction_ThrowsArgumentNullException()
        {
            var servicesMock = new Mock<IServiceCollection>();
            IServiceCollection services = servicesMock.Object;
            Func<IServiceProvider, Task<IMsalConfig>> configurator = null;

            Assert.Throws<ArgumentNullException>(() => services.AddAzureActiveDirectory(configurator));
        }
    }
}
