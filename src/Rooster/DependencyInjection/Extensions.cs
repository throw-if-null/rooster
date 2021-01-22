using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Serilog;
using Rooster.Hosting;
using Rooster.QoS.Resilency;
using Serilog;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.DependencyInjection
{
    public static class Extensions
    {
        private static readonly Action<HostBuilderContext, IServiceCollection> Empty = (_, _) => {};

        public static IHost AddHost(this IHostBuilder builder, Action<HostBuilderContext, IServiceCollection> hostConfigurator = null)
        {
            hostConfigurator ??= Empty;

            builder
                .ConfigureHostConfiguration(configurator =>
                    configurator
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false))
                .ConfigureServices((context, services) =>
                {
                    services.Configure<Collection<KuduAdapterOptions>>(context.Configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}"));
                    services.Configure<Collection<PollerOptions>>(context.Configuration.GetSection($"{nameof(PollerOptions)}"));
                    services.Configure<RetryProviderOptions>(context.Configuration.GetSection($"{nameof(RetryProviderOptions)}"));

                    services.AddMemoryCache();

                    services.AddSingleton<IRetryProvider, RetryProvider>();

                    services.AddSingleton<RecyclableMemoryStreamManager>();
                })
                .ConfigureServices(hostConfigurator)
                .ConfigureServices((ctx, services) =>
                {
                    services.AddSingleton<IInstrumentationContext, InstrumentationContext>();
                    services.AddSingleton<CorrelationIdEnricher>();

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

                        builder.AddProvider(new SerilogLoggerProvider(logger, true));

                        Log.Logger = logger;
                    });
                })
                .UseConsoleLifetime();

            return builder.Build();
        }

        public static IServiceCollection AddKuduApiAdapterCache(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var cache = new ConcurrentDictionary<string, IKuduApiAdapter>();

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                services
                    .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                    {
                        x.DefaultRequestHeaders.Add("adapter-name", option.Name.Trim().ToLowerInvariant());
                        x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                        x.BaseAddress = option.BaseUri;
                    });
            }

            using (var provider = services.BuildServiceProvider())
            {
                var adapters = provider.GetServices<IKuduApiAdapter>();

                foreach (var adapter in adapters)
                {
                    cache[adapter.Name.Trim().ToLowerInvariant()] = adapter;
                }
            }

            services.AddSingleton(new KuduApiAdapterCache(cache));

            return services;
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
