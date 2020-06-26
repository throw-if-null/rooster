using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.KuduInstances;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.DataAccess.AppServices;
using Rooster.MongoDb.DataAccess.KuduInstances;
using Rooster.MongoDb.DataAccess.Logbooks;
using Rooster.MongoDb.DataAccess.LogEntries;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.DataAccess.AppServices;
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
            return HostBuilder(args).RunConsoleAsync(BuildCancellationTokne());
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

                services.Configure<KuduAdapterOptions>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                services.Configure<AppHostOptions>(context.Configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));

                services.AddMemoryCache();

                services.AddTransient<ILogExtractor, LogExtractor>();

                var databaseEngine = context.Configuration.GetSection($"Hosts:{nameof(AppHostOptions)}").GetValue<string>("DatabaseEngine");

                switch (databaseEngine)
                {
                    case "MongoDb":
                        services = ConfigureMongo(context, services);
                        break;

                    case "SqlServer":
                        services = ConfigureSqlServer(context, services);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException($"Unknown engine: {databaseEngine}. Valid values are: MongoDb and SqlServer");
                }
            });

        private static Func<HostBuilderContext, IServiceCollection, IServiceCollection> ConfigureMongo = delegate (HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ClientFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(ClientFactoryOptions)}"));
            services.Configure<DatabaseFactoryOptions>(context.Configuration.GetSection($"{MongoConfigPath}:{nameof(DatabaseFactoryOptions)}"));
            services.Configure<AppServiceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<AppServiceCollectionFactoryOptions>()));
            services.Configure<KuduInstanceCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<KuduInstanceCollectionFactoryOptions>()));
            services.Configure<LogbookCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogbookCollectionFactoryOptions>()));
            services.Configure<LogEntryCollectionFactoryOptions>(context.Configuration.GetSection(BuildMongoCollectionFactoryConfigPath<LogEntryCollectionFactoryOptions>()));

            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<IAppServiceCollectionFactory, AppServiceCollectionFactory>();
            services.AddSingleton<IKuduInstanceCollectionFactory, KuduInstanceCollectionFactory>();
            services.AddSingleton<ILogbookCollectionFactory, LogbookCollectionFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            services
                .AddHttpClient<IKuduApiAdapter<ObjectId>, KuduApiAdapter<ObjectId>>(x => x.Timeout = TimeSpan.FromSeconds(10))
                .SetHandlerLifetime(TimeSpan.FromSeconds(5));

            services.AddTransient<IAppServiceRepository<ObjectId>, MongoDbAppServiceRepository>();
            services.AddTransient<IKuduInstanceRepository<ObjectId>, MongoDbKuduInstanceRepository>();
            services.AddTransient<ILogbookRepository<ObjectId>, MongoDbLogbookRepository>();
            services.AddTransient<ILogEntryRepository<ObjectId>, MongoDbLogEntryRepository>();

            services.AddHostedService<AppHost<ObjectId>>();

            return services;
        };

        private static Func<HostBuilderContext, IServiceCollection, IServiceCollection> ConfigureSqlServer = delegate (HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<ConnectionFactoryOptions>(context.Configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddTransient<ILogEntryRepository<int>, SqlLogEntryRepository>();
            services.AddTransient<ILogbookRepository<int>, SqlLogbookRepository>();
            services.AddTransient<IAppServiceRepository<int>, SqlAppServiceRepository>();
            services.AddTransient<IKuduInstanceRepository<int>, SqlKuduInstanceRepository>();

            services
                .AddHttpClient<IKuduApiAdapter<int>, KuduApiAdapter<int>>(x => x.Timeout = TimeSpan.FromSeconds(10))
                .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

            services.AddHostedService<AppHost<int>>();

            return services;
        };
    }
}