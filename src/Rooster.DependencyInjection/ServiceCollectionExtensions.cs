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
using Rooster.Mock.DependencyInjection;
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS.Resilency;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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

                var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                return new AuthenticationHeaderValue("Basic", base64);
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

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                services
                    .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                    {
                        x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                        x.BaseAddress = option.BaseUri;
                    });
            }

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest)
            });

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            var engines = configuration.GetSection($"{nameof(AppHostOptions)}").GetValue<string>(nameof(Engines));

            services = engines.Trim().ToUpperInvariant() switch
            {
                Engines.MongoDb => services.AddMongoDb(configuration),
                Engines.SqlServer => services.AddSqlServer(configuration),
                Engines.Slack => services.AddSlack(configuration),
                Engines.AppInsights => services.AddAppInsights(configuration),
                Engines.Mock => services.AddMock(configuration),
                _ => throw new NotSupportedEngineException(engines),
            };

            services.AddHostedService<AppHost>();

            return services;
        }
    }
}
