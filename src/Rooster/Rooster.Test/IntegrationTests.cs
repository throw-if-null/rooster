using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.DependencyInjection;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rooster.Test
{
    public class IntegrationTests
    {
        private const string SqlConfigPath = "Connectors:Sql";
        private const string MongoConfigPath = "Connectors:MongoDb";

        private static string BuildMongoCollectionFactoryConfigPath<T>()
        {
            var path = $"{MongoConfigPath}:{nameof(CollectionFactoryOptions)}:{typeof(T).Name}";

            return path;
        }

        private static readonly Func<CancellationToken> BuildCancellationTokne = delegate ()
        {
            CancellationTokenSource source = new CancellationTokenSource();

            return source.Token;
        };

        [Fact]
        public async Task ShouldThrowUnsupportedDatastoreEngineException()
        {
            var excpetion = Assert.Throws<NotSupportedDataStoreException>(() => HostBuilder(new string[0]).Build());

            Assert.Equal("Database: MySql is not supported. Supported values are: MongoDb and SqlServer.", excpetion.Message);
        }

        internal static IHostBuilder HostBuilder(string[] args) =>
          Host.CreateDefaultBuilder()
            .ConfigureHostConfiguration(configurator =>
            {
                configurator.SetBasePath(Directory.GetCurrentDirectory());
                configurator.AddJsonFile("appsettings.invalid.json", optional: true);
                configurator.AddCommandLine(args);
            })
            .ConfigureLogging((ctx, builder) =>
            {
                builder.AddConsole();
            })
            .ConfigureServices((context, services) =>
            {
                services.AddLogging(x => x.SetMinimumLevel(LogLevel.Trace));

                services.AddRooster(context.Configuration);
            });
    }
}
