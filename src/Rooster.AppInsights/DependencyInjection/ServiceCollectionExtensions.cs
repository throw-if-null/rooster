using MediatR;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.AppInsights.Handlers;
using Rooster.AppInsights.Reporters;
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

            services.AddSingleton(new TelemetryClient(new TelemetryConfiguration(instrumentationKey)));

            services.AddTransient<ITelemetryReporter, TelemetryReporter>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, AppInsightsProcessLogEntryCommand>();

            return services;
        }
    }
}