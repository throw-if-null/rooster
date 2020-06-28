using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Rooster.Adapters.Kudu;
using Rooster.Adapters.Kudu.Handlers;
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

namespace Rooster.MongoDb.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string MongoDbPath = "Connectors:MongoDb";

        private static string BuildMongoCollectionFactoryPath<T>()
        {
            return $"{MongoDbPath}:{nameof(CollectionFactoryOptions)}:{typeof(T).Name}";
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureClientFactoryOptions(configuration);
            services.ConfigureDatabaseFactoryOptions(configuration);
            services.ConfigureAppServiceCollectionFactoryOptions(configuration);
            services.ConfigureKuduInstanceCollectionFactoryOptions(configuration);
            services.ConfigureLogbookCollectionFactoryOptions(configuration);
            services.ConfigureLogEntryCollectionFactoryOptions(configuration);

            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<IAppServiceCollectionFactory, AppServiceCollectionFactory>();
            services.AddSingleton<IKuduInstanceCollectionFactory, KuduInstanceCollectionFactory>();
            services.AddSingleton<ILogbookCollectionFactory, LogbookCollectionFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            services
                .AddHttpClient<IKuduApiAdapter<ObjectId>, KuduApiAdapter<ObjectId>>()
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddTransient<IAppServiceRepository<ObjectId>, MongoDbAppServiceRepository>();
            services.AddTransient<IKuduInstanceRepository<ObjectId>, MongoDbKuduInstanceRepository>();
            services.AddTransient<ILogbookRepository<ObjectId>, MongoDbLogbookRepository>();
            services.AddTransient<ILogEntryRepository<ObjectId>, MongoDbLogEntryRepository>();

            services.AddHostedService<AppHost<ObjectId>>();

            return services;

        }

        private static IServiceCollection ConfigureClientFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection($"{MongoDbPath}:{nameof(ClientFactoryOptions)}");

            return services.Configure<ClientFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureDatabaseFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection($"{MongoDbPath}:{nameof(DatabaseFactoryOptions)}");

            return services.Configure<DatabaseFactoryOptions>(section);

        }

        private static IServiceCollection ConfigureAppServiceCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<AppServiceCollectionFactoryOptions>());

            return services.Configure<AppServiceCollectionFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureKuduInstanceCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<KuduInstanceCollectionFactoryOptions>());

            return services.Configure<KuduInstanceCollectionFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureLogbookCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<LogbookCollectionFactoryOptions>());

            return services.Configure<LogbookCollectionFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureLogEntryCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<LogEntryCollectionFactoryOptions>());

            return services.Configure<LogEntryCollectionFactoryOptions>(section);
        }
    }
}