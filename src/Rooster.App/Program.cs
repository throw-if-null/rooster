using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Docker;
using Rooster.CrossCutting.Serilog;
using Rooster.DependencyInjection;
using Rooster.DependencyInjection.Exceptions;
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
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.App
{
    internal class Program
    {
        private static readonly Func<CancellationToken> BuildCancellationToken = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static async Task Main(string[] args)
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("systemSettings.json", false)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

            var engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();

            var hosts = new List<IHost>();

            foreach (var engine in engines)
            {
                var host = engine.Trim().ToUpperInvariant() switch
                {
                    Engines.MongoDb => AddMongo(args),
                    Engines.SqlServer => AddSqlServer(args),
                    Engines.Slack => AddSlack(args),
                    Engines.AppInsights => AddAppInsights(args),
                    Engines.Mock => AddMock(args),
                    _ => throw new NotSupportedEngineException(engine),
                };

                hosts.Add(host);
            }

            var cancellation = BuildCancellationToken();

            var tasks = new List<Task>();

            foreach (var host in hosts)
            {
                tasks.Add(host.StartAsync(cancellation));
            }

            await Task.WhenAll(tasks);
        }

        private static IHost AddMongo(string[] args)
        {
            var builder = ConfigureCommon(args);

            builder.ConfigureServices((context, services) =>
            {
                services.AddMongoDb(context.Configuration);
            });

            return builder.Build();
        }

        private static IHost AddSqlServer(string[] args)
        {
            var builder = ConfigureCommon(args);

            builder.ConfigureServices((context, services) =>
            {
                services.AddSqlServer(context.Configuration);
            });

            return builder.Build();
        }

        private static IHost AddSlack(string[] args)
        {
            var builder = ConfigureCommon(args);

            builder.ConfigureServices((context, services) =>
            {
                services.AddSlack(context.Configuration);
            });

            return builder.Build();
        }

        private static IHost AddAppInsights(string[] args)
        {
            var builder = ConfigureCommon(args);

            builder.ConfigureServices((context, services) =>
            {
                services.AddAppInsights(context.Configuration);
            });

            return builder.Build();
        }

        private static IHost AddMock(string[] args)
        {
            var builder = ConfigureCommon(args);

            builder.ConfigureServices((context, services) =>
            {
                services.AddMock(context.Configuration);
            });

            return builder.Build();
        }

        private static IHostBuilder ConfigureCommon(string[] args)
        {
            var builder =
                Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(configurator =>
                configurator
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("systemSettings.json", false)
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddCommandLine(args))
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.Configure<Collection<KuduAdapterOptions>>(configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<AppHostOptions>(configuration.GetSection($"{nameof(AppHostOptions)}"));
                    services.Configure<RetryProviderOptions>(configuration.GetSection($"{nameof(RetryProviderOptions)}"));

                    services.AddMemoryCache();

                    services.AddTransient<ILogExtractor, LogExtractor>();

                    services.AddSingleton<IInstrumentationContext, InstrumentationContext>();
                    services.AddSingleton<IRetryProvider, RetryProvider>();
                    services.AddSingleton<CorrelationIdEnricher>();

                    services.AddLogging(builder =>
                    {
                        using var provider = services.BuildServiceProvider();

                        builder.ClearProviders();

                        builder.AddProvider(new SerilogLoggerProvider(
                            new LoggerConfiguration()
                            .ReadFrom.Configuration(configuration)
                            .Enrich.WithExceptionDetails()
                            .Enrich.With(new ILogEventEnricher[] { provider.GetRequiredService<CorrelationIdEnricher>() })
                            .CreateLogger(), true));
                    });

                    var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

                    foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
                    {
                        if (option.Tags.Any())
                            continue;

                        services
                            .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                            {
                                x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                                x.BaseAddress = option.BaseUri;
                            });
                    }
                });

            return builder;
        }

        private static readonly Func<string, string, AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate (string user, string password)
            {
                if (string.IsNullOrWhiteSpace(user))
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                return new AuthenticationHeaderValue("Basic", base64);
            };
    }
}