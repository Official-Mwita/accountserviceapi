using accountservice.Controllers;
using accountservice.ForcedModels;
using Microsoft.AspNetCore.Mvc;

namespace accountservice.Interfaces
{
    //This interface controls all the login functionality of this application
    public interface ILogin
    {

        //The implementation of this method persforms standard login. that's using username/email
        //and password combination
        //public Task<IActionResult> StandardLogin(UserModel user);


        //Login in user into our system by using Microsoft OpenID connect protocol
        public Task<IActionResult> LoginwithMicrosoft(string? code, string loginurl);

        //public Task<MUser> getUserInfo(string? email, string? username);

        public Task<IActionResult> HandleOAuthUserRegistration(MUser user, string auth_token, int? phonecode);

        public Task<IActionResult> GeneratePhoneCode(string phoneNumber, string token);
    }
}
