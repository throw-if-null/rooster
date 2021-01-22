using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.InitKuduPollers;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.ProcessLogSource;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Mediator.Commands.HealthCheck;
using Rooster.SqlServer.Mediator.Commands.ProcessDockerLog;
using Rooster.SqlServer.Mediator.Commands.SendDockerRunParams;
using Rooster.SqlServer.Mediator.Queries;
using System;

namespace Rooster.SqlServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SqlConfigPath = "Engines:Sql";

        public static IHost AddSqlServerHost(this IHostBuilder builder)
        {
            return builder.AddHost((ctx, services) => AddSqlServer(ctx.Configuration, services));
        }

        private static IServiceCollection AddSqlServer(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton(new HostNameEnricher(nameof(SqlServerHost)));
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddKuduApiAdapterCache(configuration);

            services.AddMediatR(new[]
            {
                typeof(ExtractDockerRunParamsRequest),
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(InitKuduPollersRequest),
                typeof(ProcessAppLogSourcesRequest),
                typeof(ProcessDockerLogRequest),
                typeof(ProcessLogSourceRequest),
                typeof(SendDockerRunParamsRequest),
                typeof(StartKuduPollerRequest),
                typeof(ValidateExportedRunParamsRequest)
            });

            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>, SqlGetLatestByServiceAndContainerNamesQuery>();
            services.AddTransient<IRequestHandler<InitKuduPollersRequest, Unit>, InitKuduPollersCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourcesRequest, Unit>, ProcessAppLogSourcesCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogRequest, Unit>, SqlProcessDockerLogCommand>();
            services.AddTransient<IRequestHandler<ProcessLogSourceRequest, Unit>, ProcessLogSourceCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();
            services.AddTransient<IRequestHandler<SendDockerRunParamsRequest, Unit>, SqlSendDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>, ValidateExportedRunParamsCommand>();

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