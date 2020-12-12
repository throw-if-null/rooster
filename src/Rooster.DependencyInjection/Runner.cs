using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Exceptions;
using Rooster.DependencyInjection;
using Rooster.HealthCheck;
using Rooster.Hosting;
using Rooster.Mock.DependencyInjection;
using Rooster.MongoDb.DependencyInjection;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster
{
    public static class Runner
    {
        private static readonly Lazy<IConfiguration> Configuration = new Lazy<IConfiguration>(() =>
        {
            IConfiguration configuration =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, true)
                    .Build();

            return configuration;
        });

        public async static Task Run(CancellationToken cancellation)
        {
            var engines = Configuration.Value.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();
            var hosts = new List<IHost>(engines.Count + 1) { Host.CreateDefaultBuilder().ConfigureHealthCheck() };

            foreach (var engine in engines)
            {
                var host = engine.Trim().ToUpperInvariant() switch
                {
                    Engines.MongoDb => Host.CreateDefaultBuilder().AddHost(services => services.AddMongoDb(Configuration.Value)),
                    Engines.SqlServer => Host.CreateDefaultBuilder().AddHost(services => services.AddSqlServer(Configuration.Value)),
                    Engines.Slack => Host.CreateDefaultBuilder().AddHost(services => services.AddSlack(Configuration.Value)),
                    Engines.AppInsights => Host.CreateDefaultBuilder().AddHost(services => services.AddAppInsights(Configuration.Value)),
                    Engines.Mock => Host.CreateDefaultBuilder().AddHost(services => services.AddMock(Configuration.Value)),
                    _ => throw new NotSupportedEngineException(engine),
                };

                hosts.Add(host);
            }

            var tasks =
                hosts.Select(host =>
                {
                    var childSource = CancellationTokenSource.CreateLinkedTokenSource(cancellation);

                    return RunHost(host, childSource.Token);
                });

            await Task.WhenAll(tasks);
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
