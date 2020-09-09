using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Hosting;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Mediator.Commands;
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

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, SqlCreateLogEntryCommand>();
            services.AddTransient<IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>, SqlGetLatestByServiceAndContainerNamesQuery>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, ProcessLogEntryCommand>();

            services.AddHostedService<AppHost<int>>();

            return services;
        }
    }
}