using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.Mediator.Commands;
using Rooster.MongoDb.Mediator.Queries;
using System;

namespace Rooster.MongoDb.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string MongoDbPath = "DataStores:MongoDb";

        private static string BuildMongoCollectionFactoryPath<T>()
        {
            return $"{MongoDbPath}:{nameof(CollectionFactoryOptions)}:{typeof(T).Name}";
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureClientFactoryOptions(configuration);
            services.ConfigureDatabaseFactoryOptions(configuration);
            services.ConfigureLogEntryCollectionFactoryOptions(configuration);

            services.AddSingleton(new HostNameEnricher(nameof(MongoDbHost)));
            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();
            services.AddKuduClient(configuration, "MONGODB");

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(CreateLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, MongoDbCreateLogEntryCommand>();
            services.AddTransient<
                IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>,
                MongoDbGetLatestByServiceAndContainerNamesQuery>();

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, ProcessLogEntryCommand>();

            services.AddHostedService<MongoDbHost>();

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