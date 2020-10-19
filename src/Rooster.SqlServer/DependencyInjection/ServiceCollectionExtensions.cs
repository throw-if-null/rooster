using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.QoS.Resilency;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Mediator.Commands.CreateLogEntry;
using Rooster.SqlServer.Mediator.Commands.HealthCheck;
using Rooster.SqlServer.Mediator.Queries;
using System;

namespace Rooster.SqlServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SqlConfigPath = "DataStores:Sql";

        public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton(new HostNameEnricher(nameof(SqlServerHost)));
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            services.AddKuduClient(configuration, "SQLSERVER");

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(CreateLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, SqlCreateLogEntryCommand>();
            services.AddTransient<IRequestHandler<
                GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>,
                SqlGetLatestByServiceAndContainerNamesQuery>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, ProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            services.AddHostedService<SqlServerHost>();

            return services;
        }

        public static IServiceCollection AddSqlServerHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddTransient<IRequestHandler<SqlServerHealthCheckRequest, HealthCheckResponse>, SqlServerHealthCheckCommand>();

            return services;
        }
    }
}