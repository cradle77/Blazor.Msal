using Des.Blazor.Msal.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Des.Blazor.Msal.Authorization
{
    public class MsalAuthenticationStateProvider : AuthenticationStateProvider
    {
        private IJSRuntime _js;
        private HttpClient _http;
        private NavigationManager _navigation;
        private ConditionalInvoker _conditionalInvoker;

        public MsalAuthenticationStateProvider(IJSRuntime js, HttpClient http, NavigationManager navigation)
        {
            _js = js;
            _http = http;
            _navigation = navigation;
            _conditionalInvoker = new ConditionalInvoker(
                () => this.AuthenticationChanged());
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var account = await _js.InvokeAsync<MsalAccount>("azuread.getAccount");

            if (account == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            Console.WriteLine(account);

            return new AuthenticationState(account.GetPrincipal());
        }

        public async Task InitializeAsync(IMsalConfig config)
        {
            var msalConfig = new
            {
                auth = new
                {
                    clientId = config.ClientId,
                    authority = config.Authority,
                    // needed to avoid the issue with iFrame src
                    redirectUri = _navigation.BaseUri
                },
                cache = new
                {
                    cacheLocation = "localStorage",
                    storeAuthStateInCookie = true
                }
            };

            Console.WriteLine("azuread.initializing");

            await _js.InvokeVoidAsync("azuread.initialize", new object[] { msalConfig });

            Console.WriteLine("azuread.initialized");
        }

        public async Task SignInAsync(params string[] scopes)
        {
            await using (await _conditionalInvoker.InvokeIfChanged(
                async () => (await this.GetAuthenticationStateAsync()).User.Identity.Name))
            {
                await _js.InvokeVoidAsync("azuread.signIn", new object[] { scopes });
            }
        }

        public async Task<MsalToken> GetAccessTokenAsync(params string[] scopes)
        {
            await using (await _conditionalInvoker.InvokeIfChanged(
                async () => (await this.GetAuthenticationStateAsync()).User.Identity.Name))
            {
                var token = await _js.InvokeAsync<MsalToken>("azuread.acquireToken",
                    new object[] { scopes });

                Console.WriteLine($"AccessToken: {token?.AccessToken}");

                return token;
            }
        }

        public async Task SignOutAsync()
        {
            await _js.InvokeVoidAsync("azuread.signOut");

            await AuthenticationChanged();
        }

        public async Task AuthenticationChanged()
        {
            Console.WriteLine("AuthenticationChanged called");

            var state = await GetAuthenticationStateAsync();

            Console.WriteLine($"AuthenticationChanged called! State is {state.User?.Identity.Name}");
            this.NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
    }
}
