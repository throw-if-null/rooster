using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Rooster.Adapters.Kudu;
using Rooster.AppHosts;
using Rooster.Connectors.MongoDb.Clients;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.Connectors.MongoDb.Databases;
using Rooster.Connectors.Sql;
using Rooster.CrossCutting;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Implementations;
using Rooster.DataAccess.KuduInstances;
using Rooster.DataAccess.KuduInstances.Implementations;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.Logbooks.Implementations;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Implementations;
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

        private static string BuildMongoCollectionFactoryConfigPath<T>()
        {
            return $"{MongoConfigPath}:{nameof(CollectionFactoryOptions)}:{nameof(T)}";
        }

        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static Task Main(string[] args)
        {
            return SqlHostBuilder(args).RunConsoleAsync(BuildCancellationTokne());
        }

        public static IHostBuilder SqlHostBuilder(string[] args) =>
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
                    services.Configure<KuduAdapterOptions>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));

                    services
                        .AddHttpClient<IKuduApiAdapter<int>, KuduApiAdapter<int>>(x => x.Timeout = TimeSpan.FromSeconds(10))
                        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

                    services.AddMemoryCache();

                    services.AddSingleton<IConnectionFactory, ConnectionFactory>();

                    services.AddTransient<ILogExtractor, LogExtractor>();
                    services.AddTransient<ILogEntryRepository<int>, SqlLogEntryRepository>();
                    services.AddTransient<ILogbookRepository<int>, SqlLogbookRepository>();
                    services.AddTransient<IAppServiceRepository<int>, SqlAppServiceRepository>();
                    services.AddTransient<IKuduInstaceRepository<int>, SqlKuduInstanceRepository>();

                    services.AddHostedService<SqlAppHost>();
                });

        public static IHostBuilder MongoDbHostBuilder(string[] args) =>
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

                    services.Configure<ClientFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(ClientFactoryOptions)}"));
                    services.Configure<DatabaseFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(DatabaseFactoryOptions)}"));
                    services.Configure<AppServiceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<AppServiceCollectionFactoryOptions>()));
                    services.Configure<KuduInstanceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<KuduInstanceCollectionFactoryOptions>()));
                    services.Configure<LogbookCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogbookCollectionFactoryOptions>()));
                    services.Configure<LogEntryCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogEntryCollectionFactoryOptions>()));

                    services.Configure<KuduAdapterOptions>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));

                    services
                        .AddHttpClient<IKuduApiAdapter<ObjectId>, KuduApiAdapter<ObjectId>>(x => x.Timeout = TimeSpan.FromSeconds(10))
                        .SetHandlerLifetime(TimeSpan.FromSeconds(5));

                    services.AddMemoryCache();

                    services.AddTransient<ILogExtractor, LogExtractor>();

                    services.AddTransient<IAppServiceRepository<ObjectId>, MongoDbAppServiceRepository>();
                    services.AddTransient<IKuduInstaceRepository<ObjectId>, MongoDbKuduInstanceRepository>();
                    services.AddTransient<ILogbookRepository<ObjectId>, MongoDbLogbookRepository>();
                    services.AddTransient<ILogEntryRepository<ObjectId>, MongoDbLogEntryRepository>();

                    services.AddHostedService<MongoDbAppHost>();
                });
    }
}