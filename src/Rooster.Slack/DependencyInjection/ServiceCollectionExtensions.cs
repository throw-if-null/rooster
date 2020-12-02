using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
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
                typeof(ShouldProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourceRequest),
                typeof(ValidateDockerRunParamsRequest),
                typeof(StartKuduPollerRequest)
            });

            services.AddTransient<IRequestHandler<ShouldProcessDockerLogRequest, Unit>, SlackProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourceRequest, Unit>, ProcessAppLogSourceCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();

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