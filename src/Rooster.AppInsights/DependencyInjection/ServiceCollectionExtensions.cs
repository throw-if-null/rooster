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
        private const string AppInsightsRootPath = "Reporters:AppInsights";

        public static IServiceCollection AddAppInsights(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<TelemetryReporterOptions>(configuration.GetSection($"{AppInsightsRootPath}:{nameof(TelemetryReporterOptions)}"));

            var instrumentationKey = configuration.GetSection($"{AppInsightsRootPath}:{nameof(TelemetryReporterOptions)}").GetValue<string>(nameof(TelemetryReporterOptions.InstrumentationKey));
            services.AddSingleton(new TelemetryClient(new TelemetryConfiguration(instrumentationKey)));

            services.AddTransient<ITelemetryReporter, TelemetryReporter>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, AppInsightsProcessLogEntryCommand>();

            return services;
        }
    }
}