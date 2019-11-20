using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    internal class Msal : IMsal
    {
        private IJSRuntime _js;
        private IConfigProvider<IMsalConfig> _configurator;
        private NavigationManager _navigation;
        private string _loginMode;

        public Msal(IJSRuntime js, NavigationManager navigation, IConfigProvider<IMsalConfig> configurator)
        {
            _js = js;
            _configurator = configurator;
            _navigation = navigation;
        }

        public bool IsInitialized { get; private set; }

        public async Task InitializeAsync()
        {
            var config = await _configurator.GetConfigurationAsync();

            var msalConfig = new
            {
                auth = new
                {
                    clientId = config.ClientId,
                    authority = config.Authority,
                    // needed to avoid the issue with iFrame src
                    redirectUri = _navigation.BaseUri,
                    navigateToLoginRequestUrl = false
                },
                cache = new
                {
                    cacheLocation = "localStorage",
                    storeAuthStateInCookie = true
                }
            };

            _loginMode = Enum.GetName(typeof(LoginModes), config.LoginMode);

            Console.WriteLine("azuread.initializing");

            await _js.InvokeVoidAsync("azuread.initialize",
                new object[] { msalConfig, DotNetObjectReference.Create(this) });

            Console.WriteLine("azuread.initialized");

            this.IsInitialized = true;
        }

        public async Task<MsalAccount> GetAccountAsync()
        {
            await this.EnsureInitializedAsync();

            return await _js.InvokeAsync<MsalAccount>("azuread.getAccount");
        }

        public async Task SignInAsync(string[] scopes)
        {
            await this.EnsureInitializedAsync();

            await _js.InvokeVoidAsync("azuread.signIn" + _loginMode, new object[] { scopes });
        }

        public async Task SignOutAsync()
        {
            await this.EnsureInitializedAsync();

            await _js.InvokeVoidAsync("azuread.signOut");
        }

        public async Task<MsalToken> AcquireTokenAsync(string[] scopes)
        {
            await this.EnsureInitializedAsync();

            return await _js.InvokeAsync<MsalToken>("azuread.acquireToken" + _loginMode,
                new object[] { scopes });
        }

        [JSInvokable]
        public void RedirectToSourceUrl(string url)
        {
            _navigation.NavigateTo(url);
        }

        private async Task EnsureInitializedAsync()
        {
            if (this.IsInitialized)
            {
                return;
            }

            await this.InitializeAsync();
        }
    }
}
