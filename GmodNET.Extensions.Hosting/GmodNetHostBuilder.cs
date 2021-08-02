using System;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Logging;

namespace GmodNET.Extensions.Hosting
{
    /// <summary>
    /// Contains static and extension methods for building Gmod.NET optimized .NET Generic Hosts.
    /// </summary>
    public static class GmodNetHostBuilder
    {
        /// <summary>
        /// Configures an existing <see cref="IHostBuilder"/> instance with pre-configured Gmod.NET defaults. 
        /// This will overwrite previously configured values and is intended to be called before additional configuration calls.
        /// </summary>
        /// <remarks>
        /// The following defaults are applied to the <see cref="IHostBuilder"/>:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// set the <see cref="IHostEnvironment.ContentRootPath"/> to the folder containing assembly of the type <typeparamref name="T"/>.
        /// </description>
        /// </item>
        /// <item><description>load host <see cref="IConfiguration"/> from "DOTNET_" prefixed environment variables</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from 'appsettings.json' and 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json'</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from User Secrets when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development' using the entry assembly</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from environment variables</description></item>
        /// <item><description>configure the <see cref="ILoggerFactory"/> to log to the <paramref name="serilogLogger"/> logger</description></item>
        /// <item><description>enables scope validation on the dependency injection container when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development'</description></item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T">The main type of the application, usually the type of the Gmod.NET module.</typeparam>
        /// <param name="builder">The existing builder to configure.</param>
        /// <param name="serilogLogger">An instance of <see cref="Serilog.ILogger"/> to use as logger.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder"/> for chaining.</returns>
        public static IHostBuilder ConfigureGmodNetDefaults<T>(this IHostBuilder builder, Serilog.ILogger serilogLogger)
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

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilder"/> class with pre-configured Gmod.NET defaults.
        /// </summary>
        /// <remarks>
        /// The following defaults are applied to the returned <see cref="HostBuilder"/>:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// set the <see cref="IHostEnvironment.ContentRootPath"/> to the folder containing assembly of the type <typeparamref name="T"/>.
        /// </description>
        /// </item>
        /// <item><description>load host <see cref="IConfiguration"/> from "DOTNET_" prefixed environment variables</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from 'appsettings.json' and 'appsettings.[<see cref="IHostEnvironment.EnvironmentName"/>].json'</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from User Secrets when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development' using the entry assembly</description></item>
        /// <item><description>load app <see cref="IConfiguration"/> from environment variables</description></item>
        /// <item><description>configure the <see cref="ILoggerFactory"/> to log to the <paramref name="serilogLogger"/> logger</description></item>
        /// <item><description>enables scope validation on the dependency injection container when <see cref="IHostEnvironment.EnvironmentName"/> is 'Development'</description></item>
        /// </list>
        /// </remarks>
        /// <typeparam name="T">The main type of the application, usually the type of the Gmod.NET module.</typeparam>
        /// <param name="serilogLogger">An instance of <see cref="Serilog.ILogger"/> to use as logger.</param>
        /// <returns>The initialized <see cref="IHostBuilder"/>.</returns>
        public static IHostBuilder CreateDefaultBuilder<T>(Serilog.ILogger serilogLogger)
        {
            HostBuilder builder = new HostBuilder();
            return builder.ConfigureGmodNetDefaults<T>(serilogLogger);
        }
    }
}
