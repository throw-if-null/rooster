using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.Connectors.MongoDb.Databases;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.KuduInstances;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster
{
    internal class Program
    {
        private const string SqlConfigPath = "Connectors:Sql";
        private const string MongoConfigPath = "Connectors:MongoDb";

        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunConsoleAsync(BuildCancellationTokne());
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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

                    services.Configure<ConnectionFactoryOptions>(context.Configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));
                    services.Configure<ConnectionFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(ConnectionFactoryOptions)}"));
                    services.Configure<DatabaseFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(DatabaseFactoryOptions)}"));
                    services.Configure<AppServiceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<AppServiceCollectionFactoryOptions>()));
                    services.Configure<KuduInstanceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<KuduInstanceCollectionFactoryOptions>()));
                    services.Configure<LogbookCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogbookCollectionFactoryOptions>()));
                    services.Configure<LogEntryCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogEntryCollectionFactoryOptions>()));

                    services.Configure<KuduAdapterOptions>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));

                    services
                        .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>(x => x.Timeout = TimeSpan.FromSeconds(10))
                        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

                    services.AddMemoryCache();

                    services.AddSingleton<IConnectionFactory, ConnectionFactory>();

                    services.AddTransient<ILogExtractor, LogExtractor>();
                    services.AddTransient<ILogEntryRepository, LogEntryRepository>();
                    services.AddTransient<ILogbookRepository, LogbookRepository>();
                    services.AddTransient<IAppServiceRepository, AppServiceRepository>();
                    services.AddTransient<IKuduInstaceRepository, KuduInstanceRepository>();

                    services.AddHostedService<AppHost>();
                });

        private static string BuildMongoCollectionFactoryConfigPath<T>()
        {
            return $"{MongoConfigPath}:{nameof(CollectionFactoryOptions)}:{nameof(T)}";
        }
    }
}