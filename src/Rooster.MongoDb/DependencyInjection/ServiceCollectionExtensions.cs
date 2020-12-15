﻿using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.ProcessLogSource;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.Mediator.Commands.HealthCheck;
using Rooster.MongoDb.Mediator.Commands.ProcessDockerLog;
using Rooster.MongoDb.Mediator.Commands.SendDockerRunParams;
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

        public static IHost AddMongoDbHost(this IHostBuilder builder)
        {
            builder.AddHost((ctx, services) => AddMongoDb(ctx.Configuration, services));

            return builder.Build();
        }

        private static IServiceCollection AddMongoDb(IConfiguration configuration, IServiceCollection services)
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
                typeof(ProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourcesRequest),
                typeof(ProcessLogSourceRequest),
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(SendDockerRunParamsRequest),
                typeof(StartKuduPollerRequest),
                typeof(ValidateExportedRunParamsRequest)
            });

            services.AddTransient<IRequestHandler<SendDockerRunParamsRequest, Unit>, MongoDbSendDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>, MongoDbGetLatestByServiceAndContainerNamesQuery>();
            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourcesRequest, Unit>, ProcessAppLogSourcesCommand>();
            services.AddTransient<IRequestHandler<ProcessLogSourceRequest, Unit>, ProcessLogSourceCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogRequest, Unit>, MongoDbProcessDockerLogCommand >();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();
            services.AddTransient<IRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>, ValidateExportedRunParamsCommand>();

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