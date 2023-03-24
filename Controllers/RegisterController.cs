
//using accountservice.ForcedModels;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections;
//using System.Data;
//using System.Data.SqlClient;

//namespace accountservice.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class RegisterController : ControllerBase
//    {


//        private readonly IConfiguration _config;
//        public RegisterController(IConfiguration config)
//        {
//            _config = config;

//        }


//        // Register A user api/<UserController>
//        [HttpPost]
//        [AllowAnonymous]
//        public async Task<IActionResult> Post([FromBody] MUser user)
//        {
//            Hashtable values = new Hashtable(); //Return values in form of a message and result

//            if (ModelState.IsValid) //Try to register user
//            {
//                //First check whether username/email is taken
//                bool userExists = await isUserExist(user.UserName, user.Email);

//                if (!userExists)//Register else
//                {
//                    bool createUser = await CreateUpdateUser(user, 0); //Zero as defualt for registration

//                    if (createUser)//Success
//                    {
//                        values.Add("Redirect", "/Login");
//                        values.Add("Success", true);

//                        return new OkObjectResult(values);

//                    }
//                    else
//                        return new UnprocessableEntityResult();


//                }
//                else
//                {
//                    values.Add("Message", "Email used or username is taken. Please try different");
//                    values.Add("Success", false);

//                    return new ConflictObjectResult(values);
//                }


//            }
//            else
//            {
//                //Return an elarate custom message
//                values.Add("Message", "Error occurred while processing your request. Try again");
//                values.Add("Success", false);
//                return new BadRequestObjectResult(values);

//            }

//        }


//        private async Task<bool> isUserExist(string username, string email)
//        {
//            try
//            {
//                //Connect to database.
//                //Return 1 if user exists, 0 if user does not and -1 if an error occured
//                using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
//                {
//                    //Connect to database then read booking records
//                    _connection.OpenAsync().Wait();

//                    using (SqlCommand command = new SqlCommand("spUserExist", _connection))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = username;
//                        command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = email;

//                        SqlDataReader reader = await command.ExecuteReaderAsync();
//                        reader.Read();

//                        int sqlresult = reader.GetInt32(0);

//                        return sqlresult != 0;

//                    }
//                }


//            }
//            catch
//            {
//                return false;
//            }

//        }


//        private async Task<bool> CreateUpdateUser(MUser user, int id)
//        {
//            //Catch error while registering
//            try
//            {
//                using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
//                {
//                    _connection.OpenAsync().Wait();

//                    using (SqlCommand command = new SqlCommand("spInsertUpdateUser", _connection))
//                    {
//                        command.CommandType = CommandType.StoredProcedure;
//                        command.Parameters.AddWithValue("userID", SqlDbType.NVarChar).Value = id;
//                        command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = user.UserName;
//                        command.Parameters.AddWithValue("password", SqlDbType.NVarChar).Value = user.Password;
//                        command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = user.Email;
//                        command.Parameters.AddWithValue("fullName", SqlDbType.NVarChar).Value = user.FullName;
//                        command.Parameters.AddWithValue("physicalAddress", SqlDbType.NVarChar).Value = user.PhysicalAddress;
//                        command.Parameters.AddWithValue("telephone", SqlDbType.NVarChar).Value = user.Telephone;
//                        command.Parameters.AddWithValue("originCountry", SqlDbType.NVarChar).Value = user.OriginCountry;
//                        command.Parameters.AddWithValue("employerName", SqlDbType.NVarChar).Value = user.EmployerName;
//                        command.Parameters.AddWithValue("experience", SqlDbType.Int).Value = user.Experience;
//                        command.Parameters.AddWithValue("position", SqlDbType.NVarChar).Value = user.Position;
//                        command.Parameters.AddWithValue("disabilityStatus", SqlDbType.NVarChar).Value = user.DisabilityStatus;
//                        command.Parameters.AddWithValue("modeused", SqlDbType.NVarChar).Value = "Microsoft";


//                        SqlDataReader reader = await command.ExecuteReaderAsync();
//                        reader.ReadAsync().Wait();

//                        if (reader.GetInt32(0) == 1) //Success


//                            return true;
//                        else

//                            return false;

//                    }

//                }



//            }
//            catch
//            {
//                //Log error message

//                //values.Add("Message", e.Message);
//                //values.Add("Success", false);
//                //return new BadRequestObjectResult(values);
//                return false;
//            }

//        }
//    }

//}
