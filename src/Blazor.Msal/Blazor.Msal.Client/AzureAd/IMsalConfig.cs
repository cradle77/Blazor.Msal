namespace Blazor.Msal.Client.AzureAd
{
    public interface IMsalConfig
    {
        string ClientId { get; }
        string Authority { get; }
    }
}
