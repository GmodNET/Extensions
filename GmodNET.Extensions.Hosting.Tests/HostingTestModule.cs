using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using GmodNET.API;
using Microsoft.Extensions.Hosting;
using Serilog;
using GmodNET.Serilog.Sink;
using System.Net.Http;

namespace GmodNET.Extensions.Hosting.Tests
{
    public class HostingTestModule : IModule
    {
        public string ModuleName => nameof(HostingTestModule);

        public string ModuleVersion => "1.0.0";

        ILogger logger;
        IHost webHost;

        string hookCallbackId;

        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.GmodSink()
                .CreateLogger();

            webHost = GmodNetHostBuilder.CreateDefaultBuilder<HostingTestModule>(logger)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSetting(WebHostDefaults.PreventHostingStartupKey, "true");
                    webBuilder.UseStartup<Startup>();
                }).Build();

            webHost.Start();

            hookCallbackId = Guid.NewGuid().ToString();

            lua.PushGlobalTable();
            lua.GetField(-1, "hook");
            lua.GetField(-1, "Add");
            lua.PushString("Tick");
            lua.PushString(hookCallbackId);
            lua.PushManagedFunction(LuaFunc);
            lua.MCall(3, 0);
            lua.Pop(2);
        }

        public void Unload(ILua lua)
        {
            webHost.StopAsync().Wait();
            webHost.Dispose();
        }

        int LuaFunc(ILua lua)
        {
            using HttpClient client = new HttpClient();
            var responce = client.GetAsync("http://localhost:5000").Result;
            string httpText = responce.Content.ReadAsStringAsync().Result;

            if (httpText == "c363f9ff-9441-4afc-ac71-ebeda3bb3c3a")
            {
                logger.Information("Test has passed!");

                lua.PushGlobalTable();
                lua.GetField(-1, "engine");
                lua.GetField(-1, "CloseServer");
                lua.MCall(0, 0);
                lua.Pop(2);
            }
            else
            {
                logger.Error("Test has FAILED!");
            }

            lua.PushGlobalTable();
            lua.GetField(-1, "hook");
            lua.GetField(-1, "Remove");
            lua.PushString("Tick");
            lua.PushString(hookCallbackId);
            lua.MCall(2, 0);
            lua.Pop(2);

            return 0;
        }
    }
}
