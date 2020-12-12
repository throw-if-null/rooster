using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Exceptions;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.Hosting;
using Rooster.Mediator.Commands.Common.Behaviors;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
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
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS.Resilency;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace Rooster.Test
{
    public static class TestRunner
    {
        internal static IHost Run(string appsettings, Action<HostBuilderContext, IServiceCollection> configure)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(appsettings, optional: false, true)
                    .Build();

            var hosts = new List<IHost>();

            var engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();

            var x = configuration.GetSection($"{nameof(AppHostOptions)}");
            foreach (var engine in engines)
            {
                var host = engine.Trim().ToUpperInvariant() switch
                {
                    Engines.MongoDb => BuildHost((ctx, services) => services.AddMongoDb(ctx.Configuration)),
                    Engines.SqlServer => BuildHost((ctx, services) => services.AddSqlServer(ctx.Configuration)),
                    Engines.Slack => BuildHost((ctx, services) => services.AddSlack(ctx.Configuration)),
                    Engines.AppInsights => BuildHost((ctx, services) => services.AddAppInsights(ctx.Configuration)),
                    Engines.Mock => BuildMockHost(configuration, configure),
                    _ => throw new NotSupportedEngineException(engine),
                };

                return host;
            }

            throw new NotImplementedException();
        }


        public static IHost BuildMockHost(IConfiguration configuration, Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            var builder =
                Host.CreateDefaultBuilder()
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
                        typeof(ProcessAppLogSourcesRequest),
                        typeof(ProcessDockerLogRequest),
                        typeof(ProcessLogSourceRequest),
                        typeof(SendDockerRunParamsRequest),
                        typeof(StartKuduPollerRequest),
                        typeof(ValidateExportedRunParamsRequest)
                    });

                    services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
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

        public static IHost BuildHost(Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            var builder =
                Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(configurator =>
                    configurator
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false))
                .ConfigureServices((context, services) => services.AddRooster(context.Configuration))
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