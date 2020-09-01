using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.Hosting;
using Rooster.Mediator.Handlers.ProcessLogEntry;
using Rooster.Slack.Handlers;
using Rooster.Slack.Reporting;
using System;

namespace Rooster.Slack.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlack(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebHookReporterOptions>(configuration.GetSection($"Connectors:Slack:{nameof(WebHookReporterOptions)}"));

            services
                .AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri("https://hooks.slack.com"))
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, SlackProcessLogEntryRequestHandler>();

            services.AddHostedService<AppHost<Nop>>();

            return services;
        }
    }
}