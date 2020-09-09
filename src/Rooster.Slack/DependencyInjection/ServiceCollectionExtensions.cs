using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Hosting;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.QoS.Intercepting;
using Rooster.Slack.Commands;
using Rooster.Slack.Reporting;
using System;

namespace Rooster.Slack.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlack(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WebHookReporterOptions>(configuration.GetSection($"Reporters:Slack:{nameof(WebHookReporterOptions)}"));

            services
                .AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri("https://hooks.slack.com"))
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, SlackProcessLogEntryCommand>();

            services.AddHostedService<AppHost<Nop>>();

            return services;
        }
    }
}