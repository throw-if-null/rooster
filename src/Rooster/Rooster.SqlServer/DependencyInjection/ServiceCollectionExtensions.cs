using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.Mediator.Requests;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.DataAccess.LogEntries;
using Rooster.SqlServer.Handlers;

namespace Rooster.SqlServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SqlConfigPath = "Connectors:Sql";

        public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddTransient<ILogEntryRepository<int>, SqlLogEntryRepository>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest<int>, Unit>, SqlProcessLogEntryRequestHandler>();

            services.AddHostedService<AppHost<int>>();

            return services;
        }
    }
}