using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessKuduLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Slack.Commands.HealthCheck;
using Rooster.Slack.Commands.LogEntryCommand;
using Rooster.Slack.Reporting;
using System;

namespace Rooster.Slack.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SlackBaseUrl = "https://hooks.slack.com";

        public static IServiceCollection AddSlack(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebHookReporterOptions>(configuration.GetSection($"Reporters:Slack:{nameof(WebHookReporterOptions)}"));

            services.AddSingleton(new HostNameEnricher(nameof(SlackHost)));

            services.AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri(SlackBaseUrl));

            services.AddKuduClient(configuration, "SLACK");

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(CreateLogEntryRequest),
                typeof(ProcessKuduLogsRequest)
            });

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, SlackProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();
            services.AddTransient<IRequestHandler<ProcessKuduLogsRequest, Unit>, ProcessKuduLogsCommand>();

            services.AddHostedService<SlackHost>();

            return services;
        }

        public static IServiceCollection AddSlackHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebHookReporterOptions>(configuration.GetSection($"Reporters:Slack:{nameof(WebHookReporterOptions)}"));

            services.AddHttpClient();

            services.AddTransient<IRequestHandler<SlackHealthCheckRequest, HealthCheckResponse>, SlackHealthCheckCommand>();

            return services;
        }
    }
}