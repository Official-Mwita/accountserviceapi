using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace accountservice.Commons
{

    //This class contains a list of common methods used across this solution. Particularly functions
    public class CommonMethods
    {


        //Returns a claim value in a list of claims based on the specified claim type
        public static string? getClaimValue(string claimtype, List<Claim> claims)
        {
            string? value = null;

            foreach (var claim in claims)
            {
                if (claim.Type == claimtype)
                {
                    return claim.Value;
                }
            }

            return value;
        }

        public static Jwt GetJWTinfo(IConfiguration configuration)
        {
            Jwt token = configuration.GetSection("Jwt").Get<Jwt>();

            return token;
        }


        //Verifies a Json token passed in the bearer header
        public static bool VerifyJwtToken(string token, string secretKey, out List<Claim> claims, string issuer, string audience)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                claims = (List<Claim>)jwtToken.Claims;
                // You can access the claims from the token using the claims variable.

                return true;
            }
            catch
            {
                claims = null;
                return false;
            }
        }

    }

    public class Jwt
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }


}
