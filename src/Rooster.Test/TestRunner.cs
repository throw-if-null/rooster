using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Hosting;
using Rooster.Mediator.Commands.Common.Behaviors;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.InitKuduPollers;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.ProcessLogSource;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.StartKuduPoller;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
using Rooster.Mock;
using Rooster.Mock.Commands.ProcessLogEntry;
using Rooster.Mock.Commands.SendDockerRunParams;
using Rooster.Mock.Reporters;
using Rooster.QoS.Resilency;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;

namespace Rooster.Test
{
    public static class TestRunner
    {
        internal static IHost Run(string appsettings, Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(appsettings, optional: false, true)
                    .Build();

            return BuildMockHost(configuration, configureHost);
        }

        public static IHost BuildMockHost(IConfiguration configuration, Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((_, services) => services.AddRooster(configuration))
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton(new HostNameEnricher(nameof(MockHost)));
                    services.AddSingleton(new ConcurrentDictionary<string, int>());

                    services.AddTransient<IMockReporter, MockReporter>();

                    services.AddKuduClient(configuration, "MOCK");

                    services.AddMediatR(new[]
                    {
                        typeof(ExtractDockerRunParamsRequest),
                        typeof(InitKuduPollersRequest),
                        typeof(ProcessAppLogSourcesRequest),
                        typeof(ProcessDockerLogRequest),
                        typeof(ProcessLogSourceRequest),
                        typeof(SendDockerRunParamsRequest),
                        typeof(StartKuduPollerRequest),
                        typeof(ValidateExportedRunParamsRequest)
                    });

                    services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
                    services.AddTransient<IRequestHandler<InitKuduPollersRequest, Unit>, InitKuduPollersCommand>();
                    services.AddTransient<IRequestHandler<ProcessAppLogSourcesRequest, Unit>, ProcessAppLogSourcesCommand>();
                    services.AddTransient<IRequestHandler<ProcessDockerLogRequest, Unit>, MockProcessDockerLogCommand>();
                    services.AddTransient<IRequestHandler<ProcessLogSourceRequest, Unit>, ProcessLogSourceCommand>();
                    services.AddTransient<IRequestHandler<SendDockerRunParamsRequest, Unit>, MockSendDockerRunParamsCommand>();
                    services.AddTransient<IRequestHandler<StartKuduPollerRequest, Unit>, StartKuduPollerCommand>();
                    services.AddTransient<IRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>, ValidateExportedRunParamsCommand>();

                    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(InstrumentingPipelineBehavior<,>));

                    services.AddHostedService<MockHost>();
                })
                .ConfigureServices(configureHost)
                .UseConsoleLifetime();

            return builder.Build();
        }

        private static IServiceCollection AddRooster(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Collection<KuduAdapterOptions>>(configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
            services.Configure<AppHostOptions>(configuration.GetSection($"{nameof(AppHostOptions)}"));
            services.Configure<RetryProviderOptions>(configuration.GetSection($"{nameof(RetryProviderOptions)}"));

            services.AddMemoryCache();

            services.AddSingleton<IInstrumentationContext, InstrumentationContext>();
            services.AddSingleton<IRetryProvider, RetryProvider>();
            services.AddSingleton<CorrelationIdEnricher>();

            services.AddKuduClient(configuration, string.Empty);

            return services;
        }
    }
}