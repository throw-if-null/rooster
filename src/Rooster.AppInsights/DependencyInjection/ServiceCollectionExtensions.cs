using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.AppInsights.Commands.HealthCheck;
using Rooster.AppInsights.Handlers.HealthCheck;
using Rooster.AppInsights.Handlers.ProcessLogEntry;
using Rooster.AppInsights.Reporters;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessKuduLogs;
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
                typeof(CreateLogEntryRequest),
                typeof(ProcessKuduLogsRequest)
            });

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, AppInsightsProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();
            services.AddTransient<IRequestHandler<ProcessKuduLogsRequest, Unit>, ProcessKuduLogsCommand>();

            services.AddHostedService<AppInsightsHost>();

            return services;
        }

        public static IServiceCollection AddAppInsightsHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IRequestHandler<AppInsightsHealthCheckRequest, HealthCheckResponse>, AppInsightsHealthCheckCommand>();

            return services;
        }
    }
}