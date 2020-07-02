using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Adapters.Kudu.Handlers;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.ContainerInstances;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using Rooster.Hosting;
using Rooster.Mediator.Notifications;
using Rooster.Services;
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

            services.AddTransient<IAppServiceRepository<object>, NullAppServiceRepository>();
            services.AddTransient<IContainerInstanceRepository<object>, NullContainerInstanceRepository>();
            services.AddTransient<ILogbookRepository<object>, NullLogbookRepository>();
            services.AddTransient<ILogEntryRepository<object>, NullLogEntryRepository>();

            services
                .AddHttpClient<IKuduApiAdapter<object>, KuduApiAdapter<object>>()
                .AddHttpMessageHandler<RequestsInterceptor>();

            services
                .AddHttpClient<IReporter, WebHookReporter>(x => x.BaseAddress = new Uri("https://hooks.slack.com"))
                .AddHttpMessageHandler<RequestsInterceptor>();

            services.AddTransient<IAppServiceService<object>, AppServiceService<object>>();
            services.AddTransient<IContainerInstanceService<object>, ContainerInstanceService<object>>();
            services.AddTransient<ILogbookService<object>, LogbookService<object>>();

            services.AddTransient<INotificationHandler<LogEntryNotification<object>>, SlackLogEntryNotificationHandler>();
            services.AddTransient<INotificationHandler<LogbookNotification<object>>, SlackLogbookNotificationHandler>();

            services.AddHostedService<AppHost<object>>();

            return services;
        }
    }
}