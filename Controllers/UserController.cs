using accountservice.Interfaces;
using accountservice.ServiceFactory;
using BookingApi.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Security.Claims;

namespace accountservice.Controllers
{
    //This controller manages authenticated user related endpoints such as get token, logout accounts, get account information
    //and many more
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _config;
        private ILogin? loginService;

        public UserController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        [Route("get_userinfo")]
        public async Task<IActionResult> GetLoginInfo()
        {
            loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);

            if (User.Identity?.IsAuthenticated ?? false)
            {

                MUser user = await loginService.getUserInfo(
                    User.FindFirst(ClaimTypes.Email)?.Value, User.FindFirst(ClaimTypes.GivenName)?.Value
                    );
                if (user != null)
                {
                    Hashtable userinfo = new Hashtable
                    {
                        { "user", user },
                        { "token", loginService.GenerateUserToken() },
                        { "status", true }
                    };

                    return new OkObjectResult(userinfo);

                }

                return BadRequest();

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpGet]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                await HttpContext.SignOutAsync("auth_cookie");

                //Try to clear all existing

                return new OkObjectResult("Successful signed out");
            }

            return new UnauthorizedResult();

        }

        [HttpGet]
        [Route("access_token")]
        public string RefreshToken()
        {
            loginService = ServicesFactory.GetLoginService(HttpContext, _config, loginService);

            return loginService.GenerateUserToken();
        }
    }
}
