using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Hosting;
using Rooster.Mediator.Handlers;
using Rooster.Mediator.Requests;
using Rooster.Mediator.Results;
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;

namespace Rooster.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRooster(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KuduAdapterOptions>(configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
            services.Configure<AppHostOptions>(configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));
            services.Configure<RetryProviderOptions>(configuration.GetSection($"{nameof(RetryProviderOptions)}"));

            services.AddMemoryCache();

            services.AddTransient<ILogExtractor, LogExtractor>();

            services.AddSingleton<IRetryProvider, RetryProvider>();
            services.AddTransient<RequestsInterceptor>();

            services.AddTransient<ILogEntryRepository<Nop>, NopLogEntryRepository>();

            services.AddHttpClient<IKuduApiAdapter, KuduApiAdapter>().AddHttpMessageHandler<RequestsInterceptor>();

            var databaseEngine = configuration.GetSection($"Hosts:{nameof(AppHostOptions)}").GetValue<string>("DatabaseEngine");

            switch (databaseEngine)
            {
                case "MongoDb":
                    services = services.AddMongoDb(configuration);
                    break;

                case "SqlServer":
                    services = services.AddSqlServer(configuration);
                    break;

                case "Slack":
                    services = services.AddSlack(configuration);
                    break;

                default:
                    throw new NotSupportedDataStoreException(databaseEngine);
            }

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest<>),
                typeof(ExportLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryRequestHandler>();

            return services;
        }
    }
}
