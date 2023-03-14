
namespace accountservice
{
    public class Program
    { 

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services);



            var app = builder.Build();
            Configure(app, builder.Environment);

            app.Run();

        }

        public static void ConfigureServices(IServiceCollection services)
        {


            // Add services to the container.
            services.AddControllers();

            //Add cookie
            services.AddAuthentication
                ("auth_cookie")
                .AddCookie("auth_cookie", ops =>
                {
                    ops.Cookie.Name = "auth_cookie";
                    ops.LoginPath = "/login";
                });

            //Adding services and their implementation
            //services.AddTransient<ILogin, Login>();

        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            //if (env.IsDevelopment())
            {
                

                app.UseCors(ops =>
                {

                    string[] origins = {
                        "https://ibusiness-git-main-moryno.vercel.app", //Dashboard frontend link
                        "https://i-business-ui-git-main-moryno.vercel.app", //login frontend link
                        "https://i-business-ui-git-main-moryno.vercel.app/",//login frontend link
                        "https://ibusiness-git-main-moryno.vercel.app/", //Dashboard frontend link
                        "http://localhost:3000",
                        "http://localhost:3000/",
                        "http://192.168.1.200:3000/",
                    };




                    ops.WithOrigins(origins).AllowCredentials().WithMethods("POST", "GET").WithHeaders("Cookie", "Content-Type", "X-Custom-Header","set-Cookie", "Authorization");
                });
            }


            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    
    }

}