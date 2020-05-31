using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.Adapters.Kudu;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using System.IO;
using System.Threading.Tasks;

namespace Rooster
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configurator =>
                {
                    configurator.SetBasePath(Directory.GetCurrentDirectory());
                    configurator.AddJsonFile("appsettings.json", optional: true);
                    configurator.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<SqlServerConnectionFactoryOptions>(context.Configuration.GetSection(nameof(SqlServerConnectionFactoryOptions)));
                    services.Configure<KuduAdapterOptions>(context.Configuration.GetSection(nameof(KuduAdapterOptions)));
                    services.Configure<AppHostOptions>(context.Configuration.GetSection(nameof(AppHostOptions)));

                    services.AddHttpClient();
                    services.AddMemoryCache();

                    services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

                    services.AddTransient<ILogExtractor, LogExtractor>();
                    services.AddTransient<ILogEntryRepository, LogEntryRepository>();
                    services.AddTransient<ILogbookRepository, LogbookRepository>();
                    services.AddTransient<IAppServiceRepository, AppServiceRepository>();
                    services.AddTransient<IKuduApiAdapter, KuduApiAdapter>();

                    services.AddHostedService<AppHost>();
                });
    }
}