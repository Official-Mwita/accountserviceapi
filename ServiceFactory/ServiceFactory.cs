using accountservice.Implementations;
using accountservice.Interfaces;

namespace accountservice.ServiceFactory
{
    public class ServicesFactory
    {

        public static ILogin GetLoginService(HttpContext httpContext, IConfiguration configuration, ILogin? loginService)
        {
            if(loginService == null)
            {
                //Create an instance of it
                ILogin service = new Login(configuration, httpContext);

                return service;
            }
            //Otherwise return the same instance

            return loginService;
        }
    }
}
