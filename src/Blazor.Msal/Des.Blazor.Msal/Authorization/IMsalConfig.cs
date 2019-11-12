namespace Des.Blazor.Msal.Authorization
{
    public interface IMsalConfig
    {
        string ClientId { get; }
        string Authority { get; }
    }
}
