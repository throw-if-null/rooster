using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.Mediator.Commands.CreateLogEntry;
using Rooster.MongoDb.Mediator.Commands.HealthCheck;
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
            services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            services.AddKuduClient(configuration, "MONGODB");

            services.AddMediatR(new[]
            {
                typeof(ShouldProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourceRequest),
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(ValidateDockerRunParamsRequest),
                typeof(StartKuduPollerRequest)
            });

            services.AddTransient<IRequestHandler<ValidateDockerRunParamsRequest, Unit>, MongoDbCreateLogEntryCommand>();
            services.AddTransient<
                IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>,
                MongoDbGetLatestByServiceAndContainerNamesQuery>();

            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourceRequest, ProcessAppLogSourceResponse>, ProcessAppLogSourceCommand>();
            services.AddTransient<IRequestHandler<ShouldProcessDockerLogRequest, Unit>, ShouldProcessDockerLogCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();

            services.AddHostedService<MongoDbHost>();

            return services;
        }

        public static IServiceCollection AddMongoDbHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureClientFactoryOptions(configuration);
            services.ConfigureDatabaseFactoryOptions(configuration);
            services.ConfigureLogEntryCollectionFactoryOptions(configuration);

            services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            services.AddTransient<IRequestHandler<MongoDbHealthCheckRequest, HealthCheckResponse>, MongoDbHealthCheckCommand>();

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