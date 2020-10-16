using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.AppInsights.Handlers;
using Rooster.AppInsights.Reporters;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;

namespace Rooster.AppInsights.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string TelemetryOptionsPath = $"Reporters:AppInsights:{nameof(TelemetryReporterOptions)}";

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelemetryReporterOptions>(configuration.GetSection(TelemetryOptionsPath));

            var instrumentationKey =
                configuration
                    .GetSection(TelemetryOptionsPath)
                    .GetValue<string>(nameof(TelemetryReporterOptions.InstrumentationKey));

            services.AddTransient<ITelemetryReporter, TelemetryReporter>();

            services.AddSingleton(new HostNameEnricher(nameof(AppInsightsHost)));
            services.AddSingleton(new TelemetryClient(new TelemetryConfiguration(instrumentationKey)));

            services.AddKuduClient(configuration, "APPINSIGHTS");

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

            services.AddHostedService<AppInsightsHost>();

            return services;
        }
    }
}