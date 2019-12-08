using System.Text.Json.Serialization;

namespace Des.Blazor.Authorization.Msal
{
    [JsonConverter(typeof(JsonStringEnumConverter))]

    public enum LoginModes
    {
        Popup,
        Redirect
    }
}