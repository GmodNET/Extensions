using System;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GmodNET.Extensions.Hosting
{
    public static class GmodNetHostBuilder
    {
        public static IHostBuilder ConfigureGmodNetDefaults<T>(this IHostBuilder builder, ILogger serilogLogger)
        {
            builder.UseContentRoot(new FileInfo(typeof(T).Assembly.Location).DirectoryName);
            builder.ConfigureHostConfiguration(config =>
            {
                config.AddEnvironmentVariables(prefix: "DOTNET_");
            });

            builder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                IHostEnvironment env = hostingContext.HostingEnvironment;
                bool reloadOnChange = GetReloadConfigOnChangeValue(hostingContext);

                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: reloadOnChange)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);

                if (env.IsDevelopment())
                {
                    config.AddUserSecrets(typeof(T).Assembly, optional: true, reloadOnChange: reloadOnChange);
                }

                config.AddEnvironmentVariables();
            })
            .UseSerilog(serilogLogger)
            .UseDefaultServiceProvider((context, options) =>
            {
                bool isDevelopment = context.HostingEnvironment.IsDevelopment();
                options.ValidateScopes = isDevelopment;
                options.ValidateOnBuild = isDevelopment;
            });

            return builder;

            [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Justification = "Calling IConfiguration.GetValue is safe when the T is bool.")]
            static bool GetReloadConfigOnChangeValue(HostBuilderContext hostingContext) => hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", defaultValue: true);
        }

        public static IHostBuilder CreateDefaultBuilder<T>(ILogger serilogLogger)
        {
            HostBuilder builder = new HostBuilder();
            return builder.ConfigureGmodNetDefaults<T>(serilogLogger);
        }
    }
}
