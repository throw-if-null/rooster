using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.AppInsights.Commands.HealthCheck;
using Rooster.AppInsights.Commands.SendDockerRunParams;
using Rooster.AppInsights.Handlers.HealthCheck;
using Rooster.AppInsights.Handlers.ProcessDockerLog;
using Rooster.AppInsights.Reporters;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.ProcessLogSource;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateExportedRunParams;

namespace Rooster.AppInsights.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly string TelemetryOptionsPath = $"Reporters:AppInsights:{nameof(TelemetryReporterOptions)}";

        public static IHost AddAppInsightsHost(this IHostBuilder builder)
        {
            builder.AddHost((ctx, services) => AddAppInsights(ctx.Configuration, services));

            return builder.Build();

        }

        private static IServiceCollection AddAppInsights(IConfiguration configuration, IServiceCollection services)
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
                typeof(ProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourcesRequest),
                typeof(ProcessLogSourceRequest),
                typeof(SendDockerRunParamsRequest),
                typeof(StartKuduPollerRequest),
                typeof(ValidateExportedRunParamsRequest)
            });

            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourcesRequest, Unit>, ProcessAppLogSourcesCommand>();
            services.AddTransient<IRequestHandler<ProcessLogSourceRequest, Unit>, ProcessLogSourceCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();
            services.AddTransient<IRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>, ValidateExportedRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogRequest, Unit>, AppInsightsProcessDockerLogCommand>();
            services.AddTransient<IRequestHandler<SendDockerRunParamsRequest, Unit>, AppInsightsSendDockerRunParamsCommand>();

            services.AddHostedService<AppInsightsHost>();

            return services;
        }

        public static IServiceCollection AddAppInsightsHealthCheck(this IServiceCollection services)
        {
            services.AddTransient<IRequestHandler<AppInsightsHealthCheckRequest, HealthCheckResponse>, AppInsightsHealthCheckCommand>();

            return services;
        }
    }
}