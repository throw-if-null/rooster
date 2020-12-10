using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.Common.Behaviors;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.ProcessLogSource;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
using Rooster.Slack.Commands.HealthCheck;
using Rooster.Slack.Commands.ProcessDockerLog;
using Rooster.Slack.Commands.SendDockerRunParams;
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
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourcesRequest),
                typeof(ProcessDockerLogRequest),
                typeof(ProcessLogSourceRequest),
                typeof(SendDockerRunParamsRequest),
                typeof(StartKuduPollerRequest),
                typeof(ValidateExportedRunParamsRequest)
            });

            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourcesRequest, Unit>, ProcessAppLogSourcesCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogRequest, Unit>, SlackProcessDockerLogCommand>();
            services.AddTransient<IRequestHandler<ProcessLogSourceRequest, Unit>, ProcessLogSourceCommand>();
            services.AddTransient<IRequestHandler<SendDockerRunParamsRequest, Unit>, SlackSendDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();
            services.AddTransient<IRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>, ValidateExportedRunParamsCommand>();

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InstrumentingPipelineBehavior<,>));

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