using Microsoft.AspNetCore.Components;

namespace Des.Blazor.Authorization.Msal.Tests
{
    internal class TestNavigationManager : NavigationManager
    {
        public TestNavigationManager(string baseUri = null, string uri = null)
        {
            Initialize(baseUri ?? "http://example.com/", uri ?? baseUri ?? "http://example.com/welcome-page");
        }

        public new void Initialize(string baseUri, string uri)
        {
            base.Initialize(baseUri, uri);
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            // do nothing
        }
    }
}