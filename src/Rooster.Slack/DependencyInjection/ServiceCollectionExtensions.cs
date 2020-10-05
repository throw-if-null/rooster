using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Slack.Commands;
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

            services.AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri(SlackBaseUrl));

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, SlackProcessLogEntryCommand>();

            return services;
        }
    }
}