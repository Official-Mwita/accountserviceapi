using accountservice.Commons;
using accountservice.Controllers;
using accountservice.ForcedModels;
using accountservice.Interfaces;
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
        //public const string APP_ADDRESS = "http://192.168.1.200:3000/";

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

                return new RedirectResult(_config.GetSection("Microsofturllocal").Get<string>());
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
                    { "redirect_uri", "http://localhost:5000/login/Loginwithmicrosoft" },
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
                            DatabaseHandler dbhander = DatabaseHandler.GetDAtabaseHandlerInstance();

                            dbhander.Parameters.Add(new Parameter
                            {
                                Name = "userName",
                                Value = " ",
                                Type = SqlDbType.NVarChar

                            });

                            dbhander.Parameters.Add(new Parameter
                            {
                                Name = "email",
                                Value = useremail ?? "empty",
                                Type = SqlDbType.NVarChar

                            });

                            using (SqlDataReader reader = await dbhander.ExecuteProcedure(_config.GetConnectionString("connString"), "spSelectUser"))
                            {

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
                                        Experience = reader.GetInt32(8),
                                        Position = reader.GetString(9),
                                        DisabilityStatus = reader.GetString(10),
                                        Password = " "

                                    };

                                    //Ensure that user is fully registered otherwise proceed
                                    bool fullyRegistered = await isFullyRegistered(loggedINUser);
                                    if (fullyRegistered)
                                    {
                                        //Now we create relevant logged in user
                                        values.Clear(); //Reset hash table values just incase

                                        values = genetrateToken(loggedINUser);


                                        return new OkObjectResult(values);
                                    }
                                    else
                                    {
                                        //Redirect to a detailed registration
                                    }


                                }

                                //Close db resource
                                dbhander.CloseResources();

                            }

                                   
                            //Initialize first user registration
                            var user = await RegisterUser(graphuserinfo);

                            if (user != null)
                            {
                                values.Clear(); //Reset hash table values just incase

                                values = genetrateToken(user);


                                return new OkObjectResult(values);

                            }



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




                    //return new RedirectResult("http://localhost:3000/login#access_token=" + jwttoken);//Redirect to react app
                }

                else
                {
                    string con = await res.Content.ReadAsStringAsync();
                    return new BadRequestObjectResult("Problem occurred while processing your request. Try again");
                }
            }

        }

        private async Task<bool> isFullyRegistered(MUser user)
        {
            bool fullyRegistered = false;

            //Get OAuth registration details
            try
            {
                using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
                {
                   await  _connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand("spSelectOAuthUser", _connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("accountID", SqlDbType.BigInt).Value = user.UserID; //Get Aouth user information by ID

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                var registrationConfirmed = reader.GetInt16(0);
                                var phoneVerified = reader.GetInt16(1);
                                

                                if(registrationConfirmed == 1 && phoneVerified == 1)
                                {
                                    fullyRegistered = true;
                                }
                                else
                                {
                                    //Register per detail
                                    //We will continue from here

                                }
                            }
                           
                        }

                    }

                }

            }
            catch (Exception)
            {
                //Handle any possible exception
                fullyRegistered = false;
            }


                return fullyRegistered;
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
                       values.Clear(); //Reset hash table values just incase

                        //clear password after use
                        loggedINUser.Password = string.Empty;

                       values = genetrateToken(loggedINUser);


                        return new OkObjectResult(values);
                       
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


        /// <summary>
        /// Generates a JWT when provided a list of claims
        /// Returns a string token
        /// </summary>
        /// <param name="claims">A list of claims</param>
        /// <returns>string token</returns>
        private string generateToken(List<Claim> claims, double validity)
        {
            string token = string.Empty;

            Jwt jwt = _config.GetSection("Jwt").Get<Jwt>();


            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key));

            SigningCredentials signin = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken securityToken = new JwtSecurityToken
            (
                jwt.Issuer,
                jwt.Audience,
                claims,
                expires: DateTime.Now.AddHours(validity), //Token expires in one hour of in activities
                signingCredentials: signin
            );

            token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            return token;
        }

        //A private function used to generate user JWT token per logged in user used across
        //All our API
        private Hashtable genetrateToken(MUser loggedINUser)
        {
            Hashtable userinfo = new Hashtable();
            //Now we create relevant logged in user
            //Creating a Jwt object
            

            //Add relevant claims
            List<Claim> claims = new List<Claim>()
                                    {
                                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),

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

            userinfo.Add("status", true);
            userinfo.Add("token", generateToken(claims, 10)); //10 hrs for a logged in token
            userinfo.Add("user", loggedINUser);

            return userinfo;
        }

        private async Task<IActionResult> HandlefullRegistration(MUser userinfo)
        {
            //Generate a short timed token to help handle data collection for this user
            //Specific stored in the token as claims
            // the information include user id (email)
            List<Claim> claims = new List<Claim>()
                                    {
                                        new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]),
                                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToString()),

                                        new Claim("principalName", userinfo.Email), //User principal id
                                        new Claim("userid", "" + userinfo.UserID) 
                                        
                                        //And many more

                                    };
            IDictionary<string, object> authorizationValues = new Dictionary<string, object>
            {
                { "auth_token", generateToken(claims, 0.6) }, //Ten minutes for code verification
                { "userinfo", userinfo }
            };

            return new OkObjectResult(authorizationValues);
 
        }

        //Phone verification method

        public async Task<IActionResult> VerifyPhone(string phoneNumber, int code, List<Claim> claims, string token="notoken")
        {
            //If code is not set use provided phone number if it exists to generate and send the code
            if(code == 0)//Generate code
            {
                Random rnd = new Random();
                code = rnd.Next(100000, 999999);
                claims.Add(new Claim("phoneCode", "" + code));//Should preferer saving the code in a database
                claims.Add(new Claim("phoneNumber", phoneNumber));

                //Send the code using preffered SMS API provideer


                //Return information 
                Dictionary<string, string> res = new Dictionary<string, string>
                {
                    { "token", generateToken(claims, 0.6) }, //Ten minutes for code verification
                    { "phoneNumber", phoneNumber }
                };

                return new OkObjectResult(res);
            }
            else //Verify code
            {
                List<Claim> verifiedClaims = new List<Claim>();

               if( CommonMethods.VerifyJwtToken(token, _config["Jwt:Key"], out verifiedClaims, _config["Jwt:Issuer"], _config["Jwt:Audience"]))
                {
                    int phoneCode;
                    int.TryParse(CommonMethods.getClaimValue("phoneCode", verifiedClaims) ?? "0", out phoneCode);
                    
                    string claimPhonenumber = CommonMethods.getClaimValue("phoneNumber", verifiedClaims)??string.Empty;

                    if(phoneCode == code && claimPhonenumber == phoneNumber)
                    {
                        //Save data then return success
                        int userID;
                        int.TryParse(CommonMethods.getClaimValue("userid", verifiedClaims) ?? "0", out userID);

                        try
                        {
                            using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
                            {
                                await _connection.OpenAsync();

                                using (SqlCommand command = new SqlCommand("spInsertUpdateAOuthUser", _connection))
                                {
                                    //Will proceed from here
                                    
                                }
                            }
                        }
                        catch
                        {
                            //Handle error request
                        }
                    }
                }

                return new UnauthorizedObjectResult("{message: Error processing your request}");

            }
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
                Experience = 0,
                Position = "Not specified",
                DisabilityStatus = "Not disabled"

            };
            using var client = new HttpClient();
            client.BaseAddress = new Uri("https://bookingapptrial.azurewebsites.net/");

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
                            Experience = reader.GetInt32(8),
                            Position = reader.GetString(9),
                            DisabilityStatus = reader.GetString(10),
                            HashPassword = reader.GetString(11)
                                                       

                        };

                        if (!is4login)
                        {
                            user.HashPassword = string.Empty;
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

    }
}
