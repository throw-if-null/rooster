using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.Handlers;
using Rooster.AppInsights.Reporters;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.AppInsights.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string TelemetryOptionsPath = $"Reporters:AppInsights:{nameof(TelemetryReporterOptions)}";

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

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelemetryReporterOptions>(configuration.GetSection(TelemetryOptionsPath));

            var instrumentationKey =
                configuration
                    .GetSection(TelemetryOptionsPath)
                    .GetValue<string>(nameof(TelemetryReporterOptions.InstrumentationKey));

            services.AddSingleton(new TelemetryClient(new TelemetryConfiguration(instrumentationKey)));

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                if (option.Tags.Any(x => x.Equals("APPINSIGHTS", StringComparison.InvariantCultureIgnoreCase)))
                    services
                        .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                        {
                            x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                            x.BaseAddress = option.BaseUri;
                        });
            }

            services.AddTransient<ITelemetryReporter, TelemetryReporter>();

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(CreateLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, AppInsightsProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            return services;
        }
    }
}