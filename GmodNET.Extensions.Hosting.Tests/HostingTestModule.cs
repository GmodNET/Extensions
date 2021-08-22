using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using GmodNET.API;
using GmodNET.Extensions.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using GmodNET.Serilog.Sink;
using System.Reflection.PortableExecutable;

namespace GmodNET.Extensions.Hosting.Tests
{
    public class HostingTestModule : IModule
    {
        public string ModuleName => nameof(HostingTestModule);

        public string ModuleVersion => "1.0.0";

        ILogger logger;
        IHost webHost;

        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.GmodSink()
                .CreateLogger();

            webHost = GmodNetHostBuilder.CreateDefaultBuilder<HostingTestModule>(logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");
                    webBuilder.UseStartup<Startup>();
                }).Build();

            webHost.Start();
        }

        public void Unload(ILua lua)
        {
            webHost.StopAsync().Wait();
            webHost.Dispose();
        }
    }
}
