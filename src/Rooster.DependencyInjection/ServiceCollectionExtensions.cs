using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Docker;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Hosting;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS.Intercepting;
using Rooster.QoS.Resilency;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
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
            services.Configure<AppHostOptions>(configuration.GetSection($"{nameof(AppHostOptions)}"));
            services.Configure<RetryProviderOptions>(configuration.GetSection($"{nameof(RetryProviderOptions)}"));

            services.AddMemoryCache();

            services.AddTransient<ILogExtractor, LogExtractor>();

            services.AddSingleton<IInstrumentationContext, InstrumentationContext>();
            services.AddSingleton<IRetryProvider, RetryProvider>();
            services.AddSingleton<CorrelationIdEnricher>();

            services.AddTransient<RequestsInterceptor>();

            services.AddLogging(builder =>
            {
                using var provider = services.BuildServiceProvider();

                builder.ClearProviders();

                builder.AddProvider(new SerilogLoggerProvider(
                    new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .Enrich.WithExceptionDetails()
                    .Enrich.With(new ILogEventEnricher[] { provider.GetRequiredService<CorrelationIdEnricher>() })
                    .CreateLogger(), true));
            });

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options)
            {
                services
                    .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid().ToString("N")}", x =>
                    {
                        x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                        x.BaseAddress = option.BaseUri;
                    })
                    .AddHttpMessageHandler<RequestsInterceptor>();
            }

            var databaseEngine = configuration.GetSection($"{nameof(AppHostOptions)}").GetValue<string>(nameof(Engine));

            services = databaseEngine.Trim().ToUpperInvariant() switch
            {
                Engine.MongoDb => services.AddMongoDb(configuration),
                Engine.SqlServer => services.AddSqlServer(configuration),
                Engine.Slack => services.AddSlack(configuration),
                Engine.AppInsights => services.AddAppInsights(configuration),
                _ => throw new NotSupportedDataStoreException(databaseEngine),
            };

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest)
            });

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            services.AddHostedService<AppHost>();

            return services;
        }
    }
}
