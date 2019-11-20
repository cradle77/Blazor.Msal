using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Moq;
using Moq.Protected;
using System.Threading.Tasks;
using Xunit;

namespace Des.Blazor.Authorization.Msal.Tests
{
    public class MsalInitializer_Tests
    {
        [Fact]
        public void WhenNotInitialized_IsInitialized_ReturnsFalse()
        {
            var js = new Mock<IJSRuntime>();
            var navigation = new Mock<NavigationManager>();
            var configurator = new Mock<IConfigProvider<IMsalConfig>>();

            var msal = new Msal(js.Object, navigation.Object, configurator.Object);

            Assert.False(msal.IsInitialized);
        }

        [Fact]
        public async Task InitializeAsync_InitializesMsal()
        {
            var js = new Mock<IJSRuntime>();
            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig();

            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);

            await msal.InitializeAsync();

            js.Verify(j => j.InvokeAsync<object>("azuread.initialize", It.IsAny<object[]>()));

            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task SignInAsync_WithPopup_CallsSignInPopup()
        {
            var js = new Mock<IJSRuntime>();
            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig()
            {
                LoginMode = LoginModes.Popup
            };

            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);
            var scopes = new string[0];
            await msal.SignInAsync(scopes);

            js.Verify(j => j.InvokeAsync<object>("azuread.signInPopup",
                It.Is<object[]>(objs => objs[0] == scopes)));

            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task SignInAsync_WithRedirect_CallsSignInRedirect()
        {
            var js = new Mock<IJSRuntime>();
            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig()
            {
                LoginMode = LoginModes.Redirect
            };

            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);
            var scopes = new string[0];
            await msal.SignInAsync(scopes);

            js.Verify(j => j.InvokeAsync<object>("azuread.signInRedirect",
                It.Is<object[]>(objs => objs[0] == scopes)));

            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task SignOutAsync_CallsSignOut()
        {
            var js = new Mock<IJSRuntime>();
            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig();
            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);

            await msal.SignOutAsync();

            js.Verify(j => j.InvokeAsync<object>("azuread.signOut", It.IsAny<object[]>()));

            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task GetAccountAsync_CallsGetAccount_ReturnsAccount()
        {
            var account = new MsalAccount();

            var js = new Mock<IJSRuntime>();
            js.Setup(j => j.InvokeAsync<MsalAccount>(It.IsAny<string>(),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<MsalAccount>(Task.FromResult(account)));

            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig();
            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);

            var result = await msal.GetAccountAsync();

            js.Verify(j => j.InvokeAsync<object>("azuread.getAccount", It.IsAny<object[]>()));

            Assert.Same(account, result);
            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task AcquireTokenAsync_WithRedirect_CallsAcquireTokenRedirect()
        {
            var token = new MsalToken();

            var js = new Mock<IJSRuntime>();
            js.Setup(j => j.InvokeAsync<MsalToken>(It.IsAny<string>(),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<MsalToken>(Task.FromResult(token)));

            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig()
            {
                LoginMode = LoginModes.Redirect
            };

            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);
            var scopes = new string[0];
            var result = await msal.AcquireTokenAsync(scopes);

            js.Verify(j => j.InvokeAsync<object>("azuread.acquireTokenRedirect",
                It.Is<object[]>(objs => objs[0] == scopes)));

            Assert.Same(token, result);

            Assert.True(msal.IsInitialized);
        }

        [Fact]
        public async Task AcquireTokenAsync_WithPopup_CallsAcquireTokenPopup()
        {
            var token = new MsalToken();

            var js = new Mock<IJSRuntime>();
            js.Setup(j => j.InvokeAsync<MsalToken>(It.IsAny<string>(),
                It.IsAny<object[]>()))
                .Returns(new ValueTask<MsalToken>(Task.FromResult(token)));

            var navigation = new TestNavigationManager();

            IMsalConfig config = new TestConfig()
            {
                LoginMode = LoginModes.Popup
            };

            var configurator = new Mock<IConfigProvider<IMsalConfig>>();
            configurator
                .Setup(x => x.GetConfigurationAsync())
                .Returns(Task.FromResult(config));

            var msal = new Msal(js.Object, navigation, configurator.Object);
            var scopes = new string[0];
            var result = await msal.AcquireTokenAsync(scopes);

            js.Verify(j => j.InvokeAsync<object>("azuread.acquireTokenPopup",
                It.Is<object[]>(objs => objs[0] == scopes)));

            Assert.Same(token, result);

            Assert.True(msal.IsInitialized);
        }
    }
}
