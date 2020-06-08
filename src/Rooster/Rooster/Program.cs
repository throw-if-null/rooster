using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.KuduInstances;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster
{
    internal class Program
    {
        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).RunConsoleAsync(BuildCancellationTokne());
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configurator =>
                {
                    configurator.SetBasePath(Directory.GetCurrentDirectory());
                    configurator.AddJsonFile("appsettings.json", optional: true);
                    configurator.AddCommandLine(args);
                })
                .ConfigureLogging((ctx, builder) =>
                {
                    builder.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddLogging(x => x.SetMinimumLevel(LogLevel.Trace));

                    services.Configure<SqlServerConnectionFactoryOptions>(context.Configuration.GetSection(nameof(SqlServerConnectionFactoryOptions)));
                    services.Configure<KuduAdapterOptions>(context.Configuration.GetSection(nameof(KuduAdapterOptions)));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection(nameof(AppHostOptions)));

                    services
                        .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>(x => x.Timeout = TimeSpan.FromSeconds(10))
                        .SetHandlerLifetime(Timeout.InfiniteTimeSpan);

                    services.AddMemoryCache();

                    services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

                    services.AddTransient<ILogExtractor, LogExtractor>();
                    services.AddTransient<ILogEntryRepository, LogEntryRepository>();
                    services.AddTransient<ILogbookRepository, LogbookRepository>();
                    services.AddTransient<IAppServiceRepository, AppServiceRepository>();
                    services.AddTransient<IKuduInstaceRepository, KuduInstanceRepository>();

                    services.AddHostedService<AppHost>();
                });
    }
}