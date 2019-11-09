using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Blazor.Msal.Client.AzureAd
{
    public class MsalAccount
    {
        public string AccountIdentifier { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public Dictionary<string, object> IdToken { get; set; }

        public override string ToString()
        {
            return this.Username;
        }

        public ClaimsPrincipal GetPrincipal()
        {
            if (string.IsNullOrEmpty(this.Username))
                return new ClaimsPrincipal();

            var claims = this.IdToken
                .Select(x => new Claim(x.Key, x.Value?.ToString()))
                .ToList();

            claims.Add(new Claim(ClaimTypes.NameIdentifier, this.Username));
            claims.Add(new Claim(ClaimTypes.Email, this.Username));

            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.NameIdentifier, ClaimTypes.Role);

            var principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}
