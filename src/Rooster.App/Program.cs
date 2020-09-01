using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.DependencyInjection;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.App
{
    class Program
    {
        private const string SqlConfigPath = "Connectors:Sql";
        private const string MongoConfigPath = "Connectors:MongoDb";

        private static string BuildMongoCollectionFactoryConfigPath<T>()
        {
            var path = $"{MongoConfigPath}:{nameof(CollectionFactoryOptions)}:{typeof(T).Name}";

            return path;
        }

        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static Task Main(string[] args)
        {
            return HostBuilder(args).UseEnvironment("Development").RunConsoleAsync(BuildCancellationTokne());
        }

        internal static IHostBuilder HostBuilder(string[] args) =>
          Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(configurator =>
            {
                configurator.SetBasePath(Directory.GetCurrentDirectory());
                configurator.AddJsonFile("appsettings.json", optional: true);
                configurator.AddCommandLine(args);
            })
            .ConfigureLogging((ctx, builder) =>
            {
                builder.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(x => x.SetMinimumLevel(LogLevel.Trace));

                services.AddRooster(context.Configuration);
            });
    }
}