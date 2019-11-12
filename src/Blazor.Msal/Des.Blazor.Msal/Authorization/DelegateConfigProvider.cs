using System;
using System.Threading.Tasks;

namespace Des.Blazor.Msal.Authorization
{
    internal class DelegateConfigProvider : IConfigProvider
    {
        private IServiceProvider _serviceProvider;
        private Func<IServiceProvider, Task<IMsalConfig>> _configurator;

        public DelegateConfigProvider(IServiceProvider serviceProvider, Func<IServiceProvider, Task<IMsalConfig>> configurator)
        {
            _serviceProvider = serviceProvider;
            _configurator = configurator;
        }

        public Task<IMsalConfig> GetConfigurationAsync()
        {
            return _configurator(_serviceProvider);
        }
    }
}
