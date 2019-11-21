namespace Des.Blazor.Authorization.Msal
{
    internal class MsalConfig : IMsalConfig
    {
        public string ClientId { get; set; }

        public string Authority { get; set; }

        public LoginModes LoginMode { get; set; }
    }
}
