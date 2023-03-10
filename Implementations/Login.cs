using accountservice.Controllers;
using accountservice.ForcedModels;
using accountservice.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace accountservice.Implementations
{
    

    public class Login : ILogin
    {
        public const string APP_ADDRESS = "https://bookingapptrial.azurewebsites.net";

        private readonly IConfiguration _config;
        private readonly HttpContext _httpContext;

        public Login(IConfiguration config, HttpContext httpContext)
        {
            _config = config;
            _httpContext = httpContext;
        }


        public async Task<IActionResult> LoginwithMicrosoft(string? code)
        {

            if (code == null)
            {
                //Generate other values such as status and status code

                return new RedirectResult(_config.GetSection("Microsofturl").Get<string>());
            }
            else
            {
                //process results
                using var client = new HttpClient();
                client.BaseAddress = new Uri("https://login.microsoftonline.com");

                Dictionary<string, string> stringbody = new Dictionary<string, string>
                {
                    { "code", code },
                    { "scope", "openid User.Read" },
                    { "client_id", _config["AzureAd:ClientId"]?? "no client id" },
                    { "redirect_uri", "http://localhost:3000/login" },
                    { "grant_type", "authorization_code" },
                    { "client_secret", _config["AzureAd:ClientSecret"]??"nosecret key" }
                };

                var content = new FormUrlEncodedContent(stringbody);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                HttpResponseMessage res = await client.PostAsync("/common/oauth2/v2.0/token", content);


                if (res.IsSuccessStatusCode)
                {

                    // ADJson? js = await res.Content.ReadFromJsonAsync<ADJson>();
                    string resBody = await res.Content.ReadAsStringAsync();
                    //JObject jObject = JObject.Parse(resBody);
                    //string? jwttoken = jObject["access_token"]?.Value<string>();

                    dynamic? obj = JsonConvert.DeserializeObject(resBody);
                    string? accesstoken = obj?.access_token;

                    //Using obtained token, extract values and check whether the user already exists.
                    //Otherwise get user information using Microsoft graph then register 'em


                    using (var client2 = new HttpClient())
                    {
                        client2.BaseAddress = new Uri("https://graph.microsoft.com");

                        client2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);

                        res = await client2.GetAsync("/v1.0/me");

                        resBody = await res.Content.ReadAsStringAsync();

                        dynamic? graphuserinfo = JsonConvert.DeserializeObject(resBody);

                        string? useremail = graphuserinfo?.userPrincipalName;



                        Hashtable values = new Hashtable();


                        //use passed in email to get user information
                        //Get Userrecord if exists
                        try
                        {

                            //Connect to database.
                            //Return 1 if email exists, 0 if user does not and -1 if an error occured
                            using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
                            {
                                //Connect to database then read booking records
                                _connection.OpenAsync().Wait();

                                using (SqlCommand command = new SqlCommand("spSelectUser", _connection))
                                {
                                    command.CommandType = CommandType.StoredProcedure;
                                    command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = " "; //We use email instead

                                    //Get email from the user claims
                                    command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = useremail ?? "empty";

                                    SqlDataReader reader = await command.ExecuteReaderAsync();

                                    if (reader.HasRows)
                                    {
                                        reader.Read();
                                        //Create a user principal. I.e login user
                                        //Check if password match then return ok
                                        //Create a user model
                                        MUser loggedINUser = new MUser()
                                        {
                                            UserID = reader.GetInt64(0),
                                            Email = reader.GetString(2),
                                            UserName = reader.GetString(1),
                                            FullName = reader.GetString(3),
                                            Telephone = reader.GetString(5),
                                            PhysicalAddress = reader.GetString(4),
                                            OriginCountry = reader.GetString(6),
                                            EmployerName = " " + reader.GetString(7),
                                            Experience = "" + reader.GetInt32(8),
                                            Position = reader.GetString(9),
                                            DisabilityStatus = reader.GetString(10),
                                            Password = " "

                                        };

                                        //Now we create relevant logged in user

                                        LoginUser(loggedINUser);

                                        return new RedirectResult("http://192.168.1.200:7030/user/access_token");
                                    }

                                   
                                    //Register user
                                    var user = await RegisterUser(graphuserinfo);

                                    if (user != null)
                                    {
                                        LoginUser(user);

                                        return new OkObjectResult("http://192.168.1.200:7030/user/access_token");

                                    }



                                    return new UnauthorizedObjectResult(values);


                                }
                            }


                        }
                        catch (Exception e)
                        {
                            values.Add("Message", e.Message);//"Error trying to process your request"
                            values.Add("Success", false);

                            return new BadRequestObjectResult(values)
                            {
                                StatusCode = 408
                            };

                        }

                    }




                    //return new RedirectResult("http://localhost:3000/login#access_token=" + jwttoken);//Redirect to react app
                }

                else
                {
                    string con = await res.Content.ReadAsStringAsync();
                    return new BadRequestObjectResult("Problem occurred while processing your request. Try again");
                }
            }

        }

        public async Task<IActionResult> StandardLogin([FromBody]UserModel user)
        {
            Hashtable values = new Hashtable();

            //try to login user if they exist
            try
            {
                MUser loggedINUser = await selectUserfromDb(user.Email, user.UserName, true);

                if(loggedINUser != null)
                {
                    //check user password
                    if (MUser.passwordHash(user.Password) == loggedINUser.Password)
                    {
                        LoginUser(loggedINUser);

                        return new RedirectResult($"{APP_ADDRESS}/user/get_userinfo");
                    }

                }
                //If we are here. There was password error, or the user doesn't exist

                //Tell user that their creadential are either wrong or do not exist
                values.Add("Message", "User does not exist or password/username incorrect");
                values.Add("status", false);

                return new UnauthorizedObjectResult(values);

            }
            catch (Exception e)
            {
                values.Add("Message", e.Message);//"Error trying to process your request"
                values.Add("Success", false);

                return new BadRequestObjectResult(values)
                {
                    StatusCode = 408
                };

            }
        }


        //This method is used to normally login a user,
        //then generate a session, that can be used by that user
        //To perform authenticated services
        private async void LoginUser(MUser loggedINUser)
        {
            //Add relevant claims
            List<Claim> claims = new List<Claim>()
                                    {
                                        new Claim(ClaimTypes.PrimarySid, ""+ loggedINUser.UserID), //User id as identified by database
                                        new Claim(ClaimTypes.GivenName, loggedINUser.UserName??""), //Username. 
                                        new Claim(ClaimTypes.Country,loggedINUser.OriginCountry), //Country 
                                        new Claim(ClaimTypes.Name, loggedINUser.FullName),
                                        new Claim(ClaimTypes.Email, loggedINUser.Email),
                                        
                                        //Source of token indicator
                                        new Claim("LocalToken", "Yes")

                                        //And many more

                                    };

            //Most importantly. If username is admin add that claim to enable admin access
            if (loggedINUser.UserName?.ToLower() == "admin")
            {
                claims.Add(new Claim(MUser.ADMIN_TYPE, loggedINUser.UserName.ToLower()));
            }

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "auth_cookie");

            ClaimsPrincipal account = new ClaimsPrincipal(claimsIdentity);


            await _httpContext.SignInAsync("auth_cookie", account);
        }


        //A private function used to generate user JWT token per logged in user used across
        //All our API
        private string genetrateToken()
        {
            //Now we create relevant logged in user
            //Creating a Jwt object
            Jwt jwt = _config.GetSection("Jwt").Get<Jwt>();

            //Add relevant claims
            List<Claim> claims = new List<Claim>()
                                    {
                                        new Claim(JwtRegisteredClaimNames.Sub, jwt.Subject),
                                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),

                                    };

            claims.AddRange(_httpContext.User.Claims);

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            SigningCredentials signin = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken
            (
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.Now.AddHours(24), //Token expires in one hour of in activities
                signingCredentials: signin
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }



        private async Task<MUser> RegisterUser(dynamic userinfo)
        {
            //We create a user model
            MUser user = new MUser
            {
                UserName = userinfo.userPrincipalName ?? "username",
                Email = userinfo.userPrincipalName ?? "mail@mail.com",
                Password = "userinfo.userPrincipalName",
                FullName = userinfo.displayName ?? "John Doe",
                PhysicalAddress = userinfo.officeLocation ?? "123 Nairobi",
                Telephone = userinfo.mobilePhone ?? "07922",
                OriginCountry = "Kenya",
                Experience = "" + 0,
                Position = "Not specified",
                DisabilityStatus = "Not disabled"

            };
            using var client = new HttpClient();
            client.BaseAddress = new Uri("http://192.168.1.200:7030");

            HttpContent body = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");

            HttpResponseMessage res = await client.PostAsync("/register", body);

            if (res.IsSuccessStatusCode)
                return user;
            else
                return null;


        }

        //Generate a user from the database either their username or email
        private async Task<MUser> selectUserfromDb(string? email, string? username, bool is4login=false)
        {
            MUser? user = null;

            //Connect to database.
            //Return 1 if email exists, 0 if user does not and -1 if an error occured
            using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
            {
                //Connect to database then read booking records
                _connection.OpenAsync().Wait();

                using (SqlCommand command = new SqlCommand("spSelectUser", _connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = username ?? "empty"; //We use email instead

                    //Get email from the user claims
                    command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = email ?? "empty";

                    SqlDataReader reader = await command.ExecuteReaderAsync() ;
                   
                    if (reader.HasRows)
                    {
                        reader.Read();
                        //Create a user principal. I.e login user
                        //Check if password match then return ok
                        //Create a user model
                        user = new MUser()
                        {
                            UserID = reader.GetInt64(0),
                            Email = reader.GetString(2),
                            UserName = reader.GetString(1),
                            FullName = reader.GetString(3),
                            Telephone = reader.GetString(5),
                            PhysicalAddress = reader.GetString(4),
                            OriginCountry = reader.GetString(6),
                            EmployerName = " " + reader.GetString(7),
                            Experience = "" + reader.GetInt32(8),
                            Position = reader.GetString(9),
                            DisabilityStatus = reader.GetString(10),
                            HashPassword = reader.GetString(11)

                        };

                        if (!is4login)
                        {
                            user.HashPassword = "-";
                        }


                        await reader.CloseAsync();

                    }

                }
            }

            //Return created user or null
            return user;

        }

        public async Task<MUser> getUserInfo(string? email, string? username)
        {
            return await selectUserfromDb(email, username);

        }

        public string GenerateUserToken()
        {
            return  genetrateToken();
        }
    }
}
