using System;
using System.Threading.Tasks;

namespace Blazor.Msal.Client.AzureAd
{
    internal class ConditionalInvoker
    {
        private Func<Task> _targetAction;

        public ConditionalInvoker(Func<Task> targetAction)
        {
            _targetAction = targetAction;
        }

        public async Task<IAsyncDisposable> InvokeIfChanged<TResult>(Func<Task<TResult>> check)
        {
            var original = await check();

            return new DisposableInvoker<TResult>(_targetAction, check, original);
        }

        private class DisposableInvoker<T> : IAsyncDisposable
        {
            private Func<Task> _targetAction;
            private Func<Task<T>> _check;
            private T _originalValue;

            public DisposableInvoker(Func<Task> targetAction, Func<Task<T>> check, T originalValue)
            {
                _targetAction = targetAction;
                _check = check;
                _originalValue = originalValue;
            }

            public async ValueTask DisposeAsync()
            {
                var finalValue = await _check();

                if (!object.Equals(finalValue, _originalValue))
                {
                    Console.WriteLine("State has changed");
                    await _targetAction();
                }
            }
        }
    }
}
