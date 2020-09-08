using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.CrossCutting;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Hosting;
using Rooster.Mediator.Handlers.ExportLogEntry;
using Rooster.Mediator.Handlers.ProcessLogEntry;
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS;
using Rooster.AppInsights.DependencyInjection;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<string, string, AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate (string user, string password)
            {
                if (string.IsNullOrWhiteSpace(user))
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                return new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}")));
            };

        public static IServiceCollection AddRooster(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Collection<KuduAdapterOptions>>(configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
            services.Configure<AppHostOptions>(configuration.GetSection($"Hosts:{nameof(AppHostOptions)}"));
            services.Configure<RetryProviderOptions>(configuration.GetSection($"{nameof(RetryProviderOptions)}"));

            services.AddMemoryCache();

            services.AddTransient<ILogExtractor, LogExtractor>();

            services.AddSingleton<IRetryProvider, RetryProvider>();
            services.AddTransient<RequestsInterceptor>();

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options)
            {
                services
                    .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>(x =>
                    {
                        x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                        x.BaseAddress = option.BaseUri;
                    })
                    .AddHttpMessageHandler<RequestsInterceptor>();
            }

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

                case "AppInsights":
                    services = services.AddAppInsights(configuration);
                    break;

                default:
                    throw new NotSupportedDataStoreException(databaseEngine);
            }

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryRequestHandler>();

            return services;
        }
    }
}
