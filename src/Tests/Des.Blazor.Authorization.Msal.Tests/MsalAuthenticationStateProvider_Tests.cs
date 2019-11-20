using Microsoft.AspNetCore.Components.Authorization;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Des.Blazor.Authorization.Msal.Tests
{
    public class MsalAuthenticationStateProvider_Tests
    {
        [Fact]
        public async Task GetAuthenticationState_WithNullAccount_ReturnsNotAuthenticated()
        {
            var msal = new Mock<IMsal>();

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            var state = await provider.GetAuthenticationStateAsync();

            Assert.False(state.User.Identity.IsAuthenticated);
        }

        [Fact]
        public async Task GetAuthenticationState_WithAccount_ReturnsAuthenticated()
        {
            var account = new MsalAccount()
            {
                Name = "Des",
                Username = "Crad",
                AccountIdentifier = "123"
            };

            var msal = new Mock<IMsal>();
            msal.Setup(x => x.GetAccountAsync())
                .Returns(Task.FromResult(account));

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            var state = await provider.GetAuthenticationStateAsync();

            Assert.True(state.User.Identity.IsAuthenticated);
            Assert.Equal(account.Username, state.User.Identity.Name);
        }

        [Fact]
        public async Task SignIn_WithSuccess_RaisesAuthenticationChanged()
        {
            var account = new MsalAccount()
            {
                Name = "Des",
                Username = "Crad",
                AccountIdentifier = "123"
            };

            var msal = new Mock<IMsal>();
            msal.SetupSequence(x => x.GetAccountAsync())
                // first call returns null
                .Returns(Task.FromResult<MsalAccount>(null))
                // second and third calls returns account
                .Returns(Task.FromResult(account))
                .Returns(Task.FromResult(account));

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            bool eventRaised = false;
            Task<AuthenticationState> stateAwaiter = null;
            
            provider.AuthenticationStateChanged += s => 
            {
                eventRaised = true;
                stateAwaiter = s;
            };

            await provider.SignInAsync();

            Assert.True(eventRaised);
            Assert.NotNull(stateAwaiter);
            var state = await stateAwaiter;
            Assert.Equal(account.Username, state.User.Identity.Name);
        }

        [Fact]
        public async Task SignIn_WithoutSuccess_DoesntRaiseAuthenticationChanged()
        {
            var msal = new Mock<IMsal>();

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            bool eventRaised = false;

            provider.AuthenticationStateChanged += s =>
            {
                eventRaised = true;
            };

            await provider.SignInAsync();

            Assert.False(eventRaised);
        }

        [Fact]
        public async Task SignIn_WithScopes_CallsMsalSignIn()
        {
            var msal = new Mock<IMsal>();

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            var scopes = new string[0];
            await provider.SignInAsync(scopes);

            msal.Verify(x => x.SignInAsync(scopes));
        }

        [Fact]
        public async Task SignOut_CallsMsalSignOut()
        {
            var msal = new Mock<IMsal>();

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            await provider.SignOutAsync();

            msal.Verify(x => x.SignOutAsync());
        }

        [Fact]
        public async Task GetAccessToken_WithTokenResponse_ReturnsToken()
        {
            var account = new MsalAccount()
            {
                Name = "Des",
                Username = "Crad",
                AccountIdentifier = "123"
            };

            var token = new MsalToken();
            var msal = new Mock<IMsal>();
            msal.Setup(x => x.AcquireTokenAsync(It.IsAny<string[]>()))
                .ReturnsAsync(token);
            msal.Setup(x => x.GetAccountAsync())
                .ReturnsAsync(account);

            var provider = new MsalAuthenticationStateProvider(msal.Object);

            var scopes = new string[0];
            var result = await provider.GetAccessTokenAsync(scopes);

            msal.Verify(x => x.AcquireTokenAsync(scopes));
            Assert.Same(token, result);
        }
    }
}
