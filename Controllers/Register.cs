using System.Data.SqlClient;
using System.Data;
using accountservice.ForcedModels;
using accountservice.Commons;

namespace accountservice.Controllers
{
    public class Register
    {

        private readonly IConfiguration _config;
        public Register(IConfiguration config)
        {
            _config = config;

        }


        // Register A user api/<UserController>
        public async Task<RegistrationResult> RegisterUser(MUser user, string modeused)
        {

                //First check whether username/email is taken
                bool userExists = await isUserExist(user.UserName??"", user.Email??"");

                if (!userExists)//Register else
                {
                    bool createUser = await CreateUpdateUser(user, 0, modeused); //Zero as defualt for registration

                    if (createUser)//Success
                    {

                         return new RegistrationResult { Message = "User successfully registered", Status = true};

                    }
                    else
                    {
                    return new RegistrationResult { Message = "Fatal Error Occured", Status = false };
                }
                        

                }
                else
                {

                    return new RegistrationResult { Message = "Email used or username is taken. Please try different", Status = false };
                 }


        }


        private async Task<bool> isUserExist(string username, string email)
        {
            try
            {
                //Connect to database.
                //Return 1 if user exists, 0 if user does not and -1 if an error occured
                using (SqlConnection _connection = new SqlConnection(_config.GetConnectionString("connString")))
                {
                    //Connect to database then read booking records
                    _connection.OpenAsync().Wait();

                    using (SqlCommand command = new SqlCommand("spUserExist", _connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("userName", SqlDbType.NVarChar).Value = username;
                        command.Parameters.AddWithValue("email", SqlDbType.NVarChar).Value = email;

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        reader.Read();

                        int sqlresult = reader.GetInt32(0);

                        return sqlresult != 0;

                    }
                }


            }
            catch
            {
                return false;
            }

        }




        private async Task<bool> CreateUpdateUser(MUser user, int id, string modeused)
        {

            bool status = false;

            //Catch error while registering
            try
            {
                DatabaseHandler dbHandler = DatabaseHandler.GetDAtabaseHandlerInstance();

                Parameter[] parameters = 
                    { new Parameter{Name =  "userID", Type =  SqlDbType.NVarChar, Value =  "" + id},
                      new Parameter{Name =  "userName", Type =  SqlDbType.NVarChar, Value =  user.UserName},
                      new Parameter{Name =  "password", Type =  SqlDbType.NVarChar, Value =  user.Password},
                      new Parameter{Name =  "email", Type =  SqlDbType.NVarChar, Value =  user.Email},
                      new Parameter{Name =  "fullName", Type =  SqlDbType.NVarChar, Value =   user.FullName},
                      new Parameter{Name =  "physicalAddress", Type =  SqlDbType.NVarChar, Value =  user.PhysicalAddress},
                      new Parameter{Name =  "telephone", Type =  SqlDbType.NVarChar, Value =   user.Telephone},
                      new Parameter{Name =  "originCountry", Type =  SqlDbType.NVarChar, Value =  user.OriginCountry},
                      new Parameter{Name =  "employerName", Type =  SqlDbType.NVarChar, Value =  user.EmployerName},
                      new Parameter{Name =  "experience", Type =  SqlDbType.Int, Value =  "" + user.Experience },
                      new Parameter{Name =  "position", Type =  SqlDbType.NVarChar, Value =  user.Position},
                      new Parameter{Name =  "disabilityStatus", Type =  SqlDbType.NVarChar, Value =  user.DisabilityStatus},
                      new Parameter{Name =  "modeused", Type =  SqlDbType.NVarChar, Value =  modeused}
                    };

                dbHandler.Parameters.AddRange(parameters);
                using (SqlDataReader reader = await dbHandler.ExecuteProcedure(_config.GetConnectionString("connString"), "spInsertUpdateUser"))
                {
                    await reader.ReadAsync();
                   
                    if (reader.GetInt32(0) == 1) //Success
                        status = true;

                    else
                       status = false;

                    dbHandler.CloseResources();

                }
            }
            catch
            {
                //Log error message

                //values.Add("Message", e.Message);
                //values.Add("Success", false);
                //return new BadRequestObjectResult(values);
                status = false;
            }

            return status;

        }
    }

    public class RegistrationResult
    {
        public bool Status { get; set;  }

        public string Message { get; set; } = string.Empty;
    }
}
