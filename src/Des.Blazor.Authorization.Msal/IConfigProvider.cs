using System.Threading.Tasks;

namespace Des.Blazor.Authorization.Msal
{
    public interface IConfigProvider<T>
    {
        Task<T> GetConfigurationAsync();
    }
}
