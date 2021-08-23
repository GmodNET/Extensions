using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace GmodNET.Extensions.Hosting.Tests
{
    public class Startup
    {
        IConfiguration config;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
           config = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.Run(async request =>
            {
                request.Response.StatusCode = 200;
                request.Response.Headers.Add("Content-Type", "text/plain");
                await request.Response.WriteAsync(config["TestString"]);
            });
        }
    }
}