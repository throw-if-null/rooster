using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Hosting;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
using Rooster.Mock;
using Rooster.Mock.Commands.ProcessLogEntry;
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
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Test
{
    public static class TestRunner
    {
        internal async static Task Run(string appsettings, Action<HostBuilderContext, IServiceCollection> configure)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile(appsettings, optional: false, true)
                    .Build();

            var hosts = new List<IHost> ();

            var engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();

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

                hosts.Add(host);
            }

            var tasks = new List<Task>();

            foreach (var host in hosts)
            {
                var childSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.None);

                tasks.Add(host.StartAsync(childSource.Token));
            }

            await Task.WhenAll(tasks);
        }


        public static IHost BuildMockHost(IConfiguration configuration, Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            var builder =
                Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => services.AddRooster(context.Configuration))
                .ConfigureServices((context, services) =>
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
                            typeof(ValidateDockerRunParamsRequest)
                        });

                    services.AddTransient<IRequestHandler<ShouldProcessDockerLogRequest, Unit>, MockProcessLogEntryCommand>();
                    services.AddTransient<IRequestHandler<ExtractDockerRunParamsRequest, ExtractDockerRunParamsResponse>, ExtractDockerRunParamsCommand>();
                    services.AddTransient<IRequestHandler<ProcessAppLogSourceRequest, Unit>, ProcessAppLogSourceCommand>();

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