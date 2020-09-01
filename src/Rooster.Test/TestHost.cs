using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.DependencyInjection;
using System;
using System.IO;

namespace Rooster.Test
{
    public static class TestHost
    {
        public static IHostBuilder Setup(
            string appSettingsName,
            Func<IServiceCollection, IConfiguration, IServiceCollection> register = null)
        {
            var host = Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(configurator =>
            {
                configurator.SetBasePath(Directory.GetCurrentDirectory());
                configurator.AddJsonFile($"{appSettingsName}", optional: true);
            })
            .ConfigureLogging((ctx, builder) =>
            {
                builder.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(x => x.SetMinimumLevel(LogLevel.Trace));

                services.AddRooster(context.Configuration);

                services = register?.Invoke(services, context.Configuration) ?? services;
            });

            return host;
        }
    }
}