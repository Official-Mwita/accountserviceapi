
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
                    //string[] origins = { 
                    //    "http://192.168.1.200:3000", 
                    //    "http://localhost:3000",
                    //    "http://192.168.1.200:3000/", 
                    //    "http://localhost:3000/",
                    //    "http://192.168.1.6:3000",
                    //    "http://192.168.1.6:3000/login",
                    //    "http://192.168.1.5:3000/*",
                    //    "https://192.168.1.5:3000/*",
                    //    "http://192.168.1.200:3000/*",
                    //};

                    string[] origins = {
                        "https://i-business-ouigcdw7x-moryno.vercel.app",
                        "https://i-business-ouigcdw7x-moryno.vercel.app/",
                        "https://ibusiness-a6vkaxgd2-moryno.vercel.app/",
                        "https://ibusiness-a6vkaxgd2-moryno.vercel.app/",
                        "http://localhost:3000",
                        "http://192.168.1.200:3000/",
                        "https://react-test-official-mwita.vercel.app/",
                        "https://react-test-eta-eight.vercel.app",
                        "https://react-test-eta-eight.vercel.app/"
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