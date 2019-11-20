using System;
using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    internal class DelegateConfigProvider<T> : IConfigProvider<T>
    {
        private IServiceProvider _serviceProvider;
        private Func<IServiceProvider, Task<T>> _configurator;

        public DelegateConfigProvider(IServiceProvider serviceProvider, Func<IServiceProvider, Task<T>> configurator)
        {
            _serviceProvider = serviceProvider;
            _configurator = configurator;
        }

        public Task<T> GetConfigurationAsync()
        {
            return _configurator(_serviceProvider);
        }
    }
}
