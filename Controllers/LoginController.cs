using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using accountservice.ServiceFactory;
using accountservice.Interfaces;
using Microsoft.AspNetCore.Authentication;
using accountservice.ForcedModels;
using Microsoft.AspNetCore.Http.Extensions;

namespace accountservice.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {

        private readonly IConfiguration _config;
        private ILogin loginService;
        public LoginController(IConfiguration config)
        {
            _config = config;

        }


        //[HttpPost]
        //public async Task<IActionResult> Post([FromBody] UserModel user)
        //{
            
        //    Hashtable values = new Hashtable();
            
        //    if (ModelState.IsValid) //Try to login user otherwise return error
        //    {
        //        loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);
                
        //        return await loginService.StandardLogin(user);

        //    }
        //    else
        //    {
        //        values.Add("Message", "Error trying to process your request");
        //        values.Add("Success", false);

        //        return new BadRequestObjectResult(values)
        //        {
        //            StatusCode = 408
        //        };
        //    }

       // } 


        

        [HttpGet]
        [Route("Loginwithmicrosoft")]
        public async Task<IActionResult> LoginwithMicrosoft(string? code)
        {
            loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);

            //Get login url
            //Should be tested against whitelist url. Though microsoft does that
            string loginurl = HttpContext.Request.GetEncodedUrl();

            int queryIndex = loginurl.IndexOf('?') < 0 ? loginurl.Length : loginurl.IndexOf('?');

            loginurl = loginurl.Substring(0, queryIndex);

            return await loginService.LoginwithMicrosoft(code, loginurl);

        }

        [HttpPost("Loginwithmicrosoft")]
        public async Task<IActionResult> LoginwithMicrosoft([FromBody] MUser user, int? phonecode)
        {
            if (ModelState.IsValid)
            {
                var authorization = HttpContext.Request.Headers.Authorization;
                if (authorization.Count > 0)
                {
                    string token = authorization[0].Substring("Bearer ".Length).Trim();

                    loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);


                    return await loginService.HandleOAuthUserRegistration(user, token, phonecode);

                }




                //User not authorized
                return Unauthorized();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet("verify_phone")]
        public async Task<IActionResult> VerifyUserPhone([FromQuery]string userphone, [FromQuery]string? code)
        {
           
            var authorization = HttpContext.Request.Headers.Authorization;
            if (authorization.Count > 0)
            {
                string token = authorization[0].Substring("Bearer ".Length).Trim();

                loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);

                //Verify phone if code exists or generate code
               
                code = code ?? string.Empty;
                if (string.IsNullOrEmpty(code))
                {
                    
                    return await loginService.GeneratePhoneCode(userphone, token);
                }
                else //Verify phone code
                {
                    int intCode;
                    int.TryParse(code ?? "0", out intCode);

                }

            }

            //User not authorized
            return Unauthorized();
        }

    }


    public class UserModel
    {
        public string Password { get; set; } = string.Empty;

        public string? UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

    }

}

   

