using Microsoft.Extensions.Hosting;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Exceptions;
using Rooster.Mock.DependencyInjection;
using Rooster.MongoDb.DependencyInjection;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;

namespace Rooster.App
{
    public static class EngineHostBuilder
    {
        public static IHost ResolveAndBuild(Engine engine)
        {
            var builder = Host.CreateDefaultBuilder();

            IHost host;

            if (engine == Engine.MongoDb)
            {
                host = builder.AddMongoDbHost();
            }
            else if (engine == Engine.SqlServer)
            {
                host = builder.AddSqlServerHost();
            }
            else if (engine == Engine.Slack)
            {
                host = builder.AddSlackHost();
            }
            else if (engine == Engine.AppInsights)
            {
                host = builder.AddAppInsightsHost();
            }
            else if (engine == Engine.Mock)
            {
                host = builder.AddMockHost();
            }
            else
            {
                throw new NotSupportedEngineException(engine.Name);
            }

            return host;
        }
    }
}
