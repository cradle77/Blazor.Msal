using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Blazor.Msal.Client.AzureAd
{
    public class MsalAuthenticationStateProvider : AuthenticationStateProvider
    {
        private IJSRuntime _js;
        private HttpClient _http;

        public MsalAuthenticationStateProvider(IJSRuntime js, HttpClient http)
        {
            _js = js;
            _http = http;
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
                    redirectUri = "https://localhost:5001/"
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

        public async Task SignIn()
        {
            await _js.InvokeVoidAsync("azuread.signIn");

            Console.WriteLine("Calling AuthenticationChanged");

            await AuthenticationChanged();
        }

        public async Task<MsalToken> GetAccessTokenAsync(params string[] scopes)
        {
            var originalState = await this.GetAuthenticationStateAsync();

            var token = await _js.InvokeAsync<MsalToken>("azuread.acquireToken",
                new object[] { scopes });
            
            // this might've changed the authentication state
            var newState = await this.GetAuthenticationStateAsync();

            if (originalState.User.Identity.Name !=
                newState.User.Identity.Name)
            {
                await AuthenticationChanged();
            }

            Console.WriteLine($"AccessToken: {token.AccessToken}");

            return token;
        }

        public async Task SignOut()
        {
            await _js.InvokeVoidAsync("azuread.signOut");

            Console.WriteLine("Calling AuthenticationChanged");

            await AuthenticationChanged();
        }

        public async Task AuthenticationChanged()
        {
            var state = await GetAuthenticationStateAsync();

            Console.WriteLine($"AuthenticationChanged called! State is {state.User?.Identity.Name}");
            this.NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
    }
}
