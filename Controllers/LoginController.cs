using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using accountservice.ServiceFactory;
using accountservice.Interfaces;
using Microsoft.AspNetCore.Authentication;

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

        [HttpGet("/free")]
        public string Free(string name)
        {
            return "I gave this for your test Mr. " + name;
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserModel user)
        {
            
            Hashtable values = new Hashtable();
            
            if (ModelState.IsValid) //Try to login user otherwise return error
            {
                loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);
                
                return await loginService.StandardLogin(user);

            }
            else
            {
                values.Add("Message", "Error trying to process your request");
                values.Add("Success", false);

                return new BadRequestObjectResult(values)
                {
                    StatusCode = 408
                };
            }

        }


        [HttpGet]
        [Route("Loginwithmicrosoft")]
        public async Task<IActionResult> LoginwithMicrosoft(string? code)
        {
            loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);

            return await loginService.LoginwithMicrosoft(code);

        }

    }


    public class UserModel
    {
        public string Password { get; set; } = string.Empty;

        public string? UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

    }
    internal class Jwt
    {
        public string Key { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
    }


}

   

