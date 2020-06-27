using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Hosting;
using Rooster.MongoDb.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;

namespace Rooster.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRooster(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KuduAdapterOptions>(configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
            services.Configure<AppHostOptions>(configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));

            services.AddMemoryCache();

            services.AddTransient<ILogExtractor, LogExtractor>();

            var databaseEngine = configuration.GetSection($"Hosts:{nameof(AppHostOptions)}").GetValue<string>("DatabaseEngine");

            switch (databaseEngine)
            {
                case "MongoDb":
                    services = services.AddMongoDb(configuration);
                    break;

                case "SqlServer":
                    services = services.AddSqlServer(configuration);
                    break;

                default:
                    throw new NotSupportedDataStoreException(databaseEngine);
            }

            return services;
        }
    }
}
