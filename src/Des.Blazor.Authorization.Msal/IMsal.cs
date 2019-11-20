using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    internal interface IMsal
    {
        Task<MsalAccount> GetAccountAsync();
        Task SignInAsync(string[] scopes);
        Task SignOutAsync();
        Task<MsalToken> AcquireTokenAsync(string[] scopes);
    }
}