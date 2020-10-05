using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rooster.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.App
{
    internal class Program
    {
        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static Task Main(string[] args)
        {
            return HostBuilder(args).RunConsoleAsync(BuildCancellationTokne());
        }

        internal static IHostBuilder HostBuilder(string[] args) =>
          Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(configurator =>
                configurator
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("systemSettings.json", false)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddCommandLine(args))
            .ConfigureServices((context, services) => services.AddRooster(context.Configuration));
    }
}