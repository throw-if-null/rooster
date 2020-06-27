using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.DependencyInjection;
using Rooster.DependencyInjection.Exceptions;
using System.IO;
using Xunit;

namespace Rooster.Test
{
    public class IntegrationTests
    {
        [Fact]
        public void ShouldThrowUnsupportedDatastoreEngineException()
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
