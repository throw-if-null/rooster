using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Hosting;
using Rooster.Mediator.Commands.Requests;
using Rooster.Mediator.Queries.Requests;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Mediator.Commands;
using Rooster.SqlServer.Mediator.Queries;
using System;

namespace Rooster.SqlServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SqlConfigPath = "Connectors:Sql";

        public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, SqlCreateLogEntryCommand>();
            services.AddTransient<IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>, SqlGetLatestByServiceAndContainerNamesQuery>();

            services.AddHostedService<AppHost<int>>();

            return services;
        }
    }
}