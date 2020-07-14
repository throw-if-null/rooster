using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.Mediator.Requests;
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

            services.AddTransient<ILogEntryRepository<object>, NullLogEntryRepository>();

            services
                .AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri("https://hooks.slack.com"))
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddTransient<IRequestHandler<ExportLogEntryRequest<object>, ProcessLogEntryRequest<object>>, SlackExportLogEntryRequestHandler>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest<object>, Unit>, SlackProcessLogEntryRequestHandler>();

            services.AddHostedService<AppHost<object>>();

            return services;
        }
    }
}