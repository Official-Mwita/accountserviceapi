using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
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

        //Verifies a Json token passed in the bearer header
        //It first decrypt the token before passing it to the original verify jwt token (plain)
        public static bool VerifyEncyptedJwtToken(string token, string secret, out List<Claim> claims, string issuer, string audience, IConfiguration config)
        {
            string decrptedToken = new AesEncryption(config).Decrypt(token);//Token supplied is encrypted decrypt it first

            return VerifyJwtToken(decrptedToken, secret, out claims, issuer, audience);
        }

        /// <summary>
        /// This method is used to send a generic OTP message to phone number owner
        /// </summary>
        /// <param name="phonenumber">Phone number of the user</param>
        /// <param name="code">One time generated code</param>
        /// <returns>true or false depending on API provider response success</returns>
        public static async Task<bool> sendOtpMessage(string phonenumber, int code)
        {
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.sandbox.africastalking.com");

            Dictionary<string, string> postData = new Dictionary<string, string>
                {
                    { "username", "sandbox" },
                    { "to", phonenumber },
                    { "message", $"Your phone verification code is {code}. It should not be shared" },
                    { "from", "joseph" }
                };

            var content = new FormUrlEncodedContent(postData);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            //Custom headers
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("apiKey", "6b39d1e1220520e89de3cf72402a5ec69c99ac66d29cd10bea28a8dd1aa24b5a");

            HttpResponseMessage res = await client.PostAsync("/version1/messaging", content);


            //// ADJson? js = await res.Content.ReadFromJsonAsync<ADJson>();
            //string resBody = await res.Content.ReadAsStringAsync();

            //Console.WriteLine(resBody);

            return res.IsSuccessStatusCode;
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
