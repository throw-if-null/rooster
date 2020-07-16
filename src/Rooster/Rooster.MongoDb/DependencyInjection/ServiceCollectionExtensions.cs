using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.Mediator.Handlers;
using Rooster.Mediator.Requests;
using Rooster.Mediator.Results;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.DataAccess.LogEntries;
using Rooster.MongoDb.Handlers;

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
            services.ConfigureLogEntryCollectionFactoryOptions(configuration);

            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            services.AddTransient<ILogEntryRepository<ObjectId>, MongoDbLogEntryRepository>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest<ObjectId>, Unit>, MongoDbProcessLogEntryRequestHandler>();

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

        private static IServiceCollection ConfigureLogEntryCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<LogEntryCollectionFactoryOptions>());

            return services.Configure<LogEntryCollectionFactoryOptions>(section);
        }
    }
}