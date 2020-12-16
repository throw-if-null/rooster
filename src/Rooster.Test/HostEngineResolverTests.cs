using Microsoft.Extensions.Hosting;
using Rooster.App;
using Rooster.AppInsights;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Exceptions;
using Rooster.Mock;
using Rooster.MongoDb;
using Rooster.Slack;
using Rooster.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rooster.Test
{
    public class HostEngineResolverTests
    {
        [Theory]
        [InlineData("mongodb")]
        [InlineData("sqlserver")]
        [InlineData("slack")]
        [InlineData("appinsights")]
        [InlineData("mock")]
        public void Should_create_engine(string engineName)
        {
            Engine.FromName(engineName);
        }

        [Theory]
        [InlineData("splunk")]
        [InlineData("mysql")]
        [InlineData("something")]
        [InlineData("")]
        [InlineData(null)]
        public void Should_throw_NotSupportedEngineException_when_creating_engine(string engineName)
        {
            Assert.Throws<NotSupportedEngineException>(() => Engine.FromName(engineName));
        }

        [Theory]
        [InlineData(nameof(Engine.AppInsights), nameof(AppInsightsHost))]
        [InlineData(nameof(Engine.Mock), nameof(MockHost))]
        [InlineData(nameof(Engine.MongoDb), nameof(MongoDbHost))]
        [InlineData(nameof(Engine.Slack), nameof(SlackHost))]
        [InlineData(nameof(Engine.SqlServer), nameof(SqlServerHost))]
        public void Should_build_supported_hosts(string engineName, string hostName)
        {
            var engine = Engine.FromName(engineName);

            var host = EngineHostBuilder.ResolveAndBuild(engine);
            var appHost = host.Services.GetService(typeof(IHostedService));

            Assert.NotNull(appHost);
            Assert.Equal(hostName, appHost.GetType().Name);

            host.Dispose();
        }

        [Fact]
        public void Should_throw_NotSupportedEngineException_when_creating_host()
        {
            Assert.Throws<NotSupportedEngineException>(() => EngineHostBuilder.ResolveAndBuild(Engine.Unsupported));
        }
    }
}
