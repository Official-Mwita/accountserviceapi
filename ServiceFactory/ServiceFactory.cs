using accountservice.Implementations;
using accountservice.Interfaces;
using Microsoft.AspNetCore.DataProtection;

namespace accountservice.ServiceFactory
{
    public class ServicesFactory
    {

        public static ILogin GetLoginService(HttpContext httpContext, IConfiguration configuration, ILogin? loginService, IDataProtectionProvider idp)
        {
            if(loginService == null)
            {
                //Create an instance of it
                ILogin service = new Login(configuration, httpContext, idp);

                return service;
            }
            //Otherwise return the same instance

            return loginService;
        }
    }
}
