using Blazor.Msal.Client.AzureAd;

namespace Blazor.Msal.Client
{
    public class ClientConfig : IMsalConfig
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
    }
}
