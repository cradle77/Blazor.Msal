namespace Des.Blazor.Authorization.Msal
{
    public interface IMsalConfig
    {
        string ClientId { get; }
        string Authority { get; }
    }
}
