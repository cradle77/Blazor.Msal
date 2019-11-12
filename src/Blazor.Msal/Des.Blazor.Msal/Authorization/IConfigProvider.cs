using System.Threading.Tasks;

namespace Des.Blazor.Msal.Authorization
{
    public interface IConfigProvider
    {
        Task<IMsalConfig> GetConfigurationAsync();
    }
}
