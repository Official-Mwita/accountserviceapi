﻿using accountservice.Controllers;
using BookingApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace accountservice.Interfaces
{
    //This interface controls all the login functionality of this application
    public interface ILogin
    {

        //The implementation of this method persforms standard login. that's using username/email
        //and password combination
        public Task<IActionResult> StandardLogin(UserModel user);


        //Login in user into our system by using Microsoft OpenID connect protocol
        public Task<IActionResult> LoginwithMicrosoft(string? code);
        public string GenerateUserToken();

        public Task<MUser> getUserInfo(string? email, string? username);
    }
}
