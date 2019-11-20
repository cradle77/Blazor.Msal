using Des.Blazor.Msal.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    internal class MsalAuthenticationStateProvider : AuthenticationStateProvider, IAuthenticationManager
    {
        private ConditionalInvoker _conditionalInvoker;
        private IMsal _msal;

        public MsalAuthenticationStateProvider(IMsal msal)
        {
            _conditionalInvoker = new ConditionalInvoker(
                () => this.AuthenticationChangedAsync());
            _msal = msal;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var account = await _msal.GetAccountAsync();

            if (account == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            Console.WriteLine(account);

            return new AuthenticationState(account.GetPrincipal());
        }

        public async Task SignInAsync(params string[] scopes)
        {
            await using (await _conditionalInvoker.InvokeIfChanged(
                async () => (await this.GetAuthenticationStateAsync()).User.Identity.Name))
            {
                await _msal.SignInAsync(scopes);
            }
        }

        public async Task<MsalToken> GetAccessTokenAsync(params string[] scopes)
        {
            await using (await _conditionalInvoker.InvokeIfChanged(
                async () => (await this.GetAuthenticationStateAsync()).User.Identity.Name))
            {
                var token = await _msal.AcquireTokenAsync(scopes);

                Console.WriteLine($"AccessToken: {token?.AccessToken}");

                return token;
            }
        }

        public async Task SignOutAsync()
        {
            await _msal.SignOutAsync();

            await AuthenticationChangedAsync();
        }

        private async Task AuthenticationChangedAsync()
        {
            Console.WriteLine("AuthenticationChanged called");

            var state = await GetAuthenticationStateAsync();

            Console.WriteLine($"AuthenticationChanged called! State is {state.User?.Identity.Name}");
            this.NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
    }
}
