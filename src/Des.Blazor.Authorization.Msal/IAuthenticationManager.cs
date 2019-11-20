using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    public interface IAuthenticationManager
    {
        Task SignInAsync(params string[] scopes);

        Task<MsalToken> GetAccessTokenAsync(params string[] scopes);

        Task SignOutAsync();
    }
}