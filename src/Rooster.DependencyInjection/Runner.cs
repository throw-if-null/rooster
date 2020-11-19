using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection.Exceptions;
using Rooster.HealthCheck;
using Rooster.Hosting;
using Rooster.Mock.DependencyInjection;
using Rooster.MongoDb.DependencyInjection;
using Rooster.QoS.Resilency;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DependencyInjection
{
    public static class Runner
    {
        public static Task Run(CancellationToken cancellation)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, true)
                    .Build();

            return Run(configuration, cancellation);
        }

        public async static Task Run(IConfiguration configuration, CancellationToken cancellation)
        {
            IHost webHost = Host.CreateDefaultBuilder().ConfigureHealthCheck();
            var hosts = new List<IHost> { webHost };

            var engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();

            foreach (var engine in engines)
            {
                var host = engine.Trim().ToUpperInvariant() switch
                {
                    Engines.MongoDb => BuildHost((ctx, services) => services.AddMongoDb(ctx.Configuration)),
                    Engines.SqlServer => BuildHost((ctx, services) => services.AddSqlServer(ctx.Configuration)),
                    Engines.Slack => BuildHost((ctx, services) => services.AddSlack(ctx.Configuration)),
                    Engines.AppInsights => BuildHost((ctx, services) => services.AddAppInsights(ctx.Configuration)),
                    Engines.Mock => BuildHost((ctx, services) => services.AddMock(ctx.Configuration)),
                    _ => throw new NotSupportedEngineException(engine),
                };

                hosts.Add(host);
            }

            var tasks = new List<Task>(hosts.Count);

            foreach (var host in hosts)
            {
                var childSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

                tasks.Add(RunHost(host, childSource.Token));
            }

            await Task.WhenAll(tasks);
        }

        private static IHost BuildHost(Action<HostBuilderContext, IServiceCollection> configureHost)
        {
            var builder =
                Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(configurator =>
                configurator
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false))
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Collection<KuduAdapterOptions>>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection($"{nameof(AppHostOptions)}"));
                    services.Configure<RetryProviderOptions>(context.Configuration.GetSection($"{nameof(RetryProviderOptions)}"));

                    services.AddMemoryCache();

                    services.AddSingleton<IInstrumentationContext, InstrumentationContext>();
                    services.AddSingleton<IRetryProvider, RetryProvider>();
                    services.AddSingleton<CorrelationIdEnricher>();

                    services.AddSingleton<RecyclableMemoryStreamManager>();
                })
                .ConfigureServices(configureHost)
                .ConfigureServices((ctx, services) =>
                {
                    services.AddLogging(builder =>
                    {
                        using var provider = services.BuildServiceProvider();

                        builder.ClearProviders();

                        var logger = new LoggerConfiguration()
                            .ReadFrom.Configuration(ctx.Configuration)
                            .Enrich.WithExceptionDetails()
                            .Enrich.With(new ILogEventEnricher[]
                            {
                                provider.GetRequiredService<CorrelationIdEnricher>(),
                                provider.GetRequiredService<HostNameEnricher>()
                            })
                            .CreateLogger();

                        builder.AddProvider(new SerilogLoggerProvider(
                            logger, true));

                        Log.Logger = logger;
                    });
                })
                .UseConsoleLifetime();

            return builder.Build();
        }

        private static async Task RunHost(IHost host, CancellationToken cancellation)
        {
            try
            {
                await host.RunAsync(cancellation);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Host failed.", ex);
            }
        }
    }
}
