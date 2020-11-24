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
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateDockerRunParams;

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
                typeof(ShouldProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourceRequest),
                typeof(ValidateDockerRunParamsRequest),
                typeof(StartKuduPollerRequest)
            });

            services.AddTransient<IRequestHandler<ShouldProcessDockerLogRequest, Unit>, AppInsightsProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourceRequest, ProcessAppLogSourceResponse>, ProcessAppLogSourceCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();

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