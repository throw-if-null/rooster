using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
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
            services.AddSingleton(new ConcurrentBag<ShouldProcessDockerLogRequest>());

            services.AddTransient<IMockReporter, MockReporter>();

            services.AddKuduClient(configuration, "MOCK");

            services.AddMediatR(new[]
            {
                typeof(ShouldProcessDockerLogRequest),
                typeof(ExtractDockerRunParamsRequest),
                typeof(ProcessAppLogSourceRequest),
                typeof(ValidateDockerRunParamsRequest),
                typeof(StartKuduPollerRequest)
            });

            services.AddTransient<IRequestHandler<ShouldProcessDockerLogRequest, Unit>, MockProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
            services.AddTransient<IRequestHandler<ProcessAppLogSourceRequest, Unit>, ProcessAppLogSourceCommand>();
            services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();

            services.AddHostedService<MockHost>();

            return services;
        }
    }
}