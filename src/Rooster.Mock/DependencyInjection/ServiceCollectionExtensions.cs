using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessKuduLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mock.Commands.ProcessLogEntry;
using Rooster.Mock.Reporters;
using System.Collections.Concurrent;

namespace Rooster.Mock.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMock(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new HostNameEnricher(nameof(MockHost)));
            services.AddSingleton(new ConcurrentBag<ProcessLogEntryRequest>());

            services.AddTransient<IMockReporter, MockReporter>();

            services.AddKuduClient(configuration, "MOCK");

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(CreateLogEntryRequest),
                typeof(ProcessKuduLogsRequest)
            });

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, MockProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();
            services.AddTransient<IRequestHandler<ProcessKuduLogsRequest, Unit>, ProcessKuduLogsCommand>();

            services.AddHostedService<MockHost>();

            return services;
        }
    }
}