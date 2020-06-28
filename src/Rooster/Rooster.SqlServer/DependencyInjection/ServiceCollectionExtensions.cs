using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.KuduInstances;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.DataAccess.AppServices;
using Rooster.SqlServer.DataAccess.KuduInstances;
using Rooster.SqlServer.DataAccess.Logbooks;
using Rooster.SqlServer.DataAccess.LogEntries;

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
            services.AddTransient<ILogbookRepository<int>, SqlLogbookRepository>();
            services.AddTransient<IAppServiceRepository<int>, SqlAppServiceRepository>();
            services.AddTransient<IKuduInstanceRepository<int>, SqlKuduInstanceRepository>();

            services
                .AddHttpClient<IKuduApiAdapter<int>, KuduApiAdapter<int>>()
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddHostedService<AppHost<int>>();

            return services;
        }
    }
}