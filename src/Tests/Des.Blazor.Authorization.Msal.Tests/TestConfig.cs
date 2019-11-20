namespace Des.Blazor.Authorization.Msal.Tests
{
    internal class TestConfig : IMsalConfig
    {
        public string ClientId { get; set; } = "12345";

        public string Authority { get; set; } = "https://myauthority.com/";

        public LoginModes LoginMode { get; set; }
    }
}
