using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace SportServer.Models
{
    public class JwtOptions
    {
        private string secretKey { get; set; }

        public SymmetricSecurityKey SymmetricSecurityKey => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public JwtOptions(string secretKey, string issuer, string audience)
        {
            this.secretKey = secretKey;
            Issuer = issuer;
            Audience = audience;
        }

    }
}
