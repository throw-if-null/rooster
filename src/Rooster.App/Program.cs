using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rooster.AppInsights.DependencyInjection;
using Rooster.CrossCutting;
using Rooster.CrossCutting.Exceptions;
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

internal class Program
{
    internal static Task Main()
    {
        return Run(new CancellationTokenSource().Token);
    }

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
        var engines = Configuration.Value.GetSection($"{nameof(AppHostOptions)}:{nameof(Engine)}").Get<Collection<string>>();
        var hosts = new List<IHost>(engines.Count + 1) { Host.CreateDefaultBuilder().ConfigureHealthCheck() };

        foreach (var engine in Engine.ToList(engines))
        {
            IHost host = null;

            if (engine.Equals(Engine.MongoDb))
            {
                host = Host.CreateDefaultBuilder().AddMongoDbHost();
            }
            else if (engine.Equals(Engine.SqlServer))
            {
                host = Host.CreateDefaultBuilder().AddSqlServerHost();
            }
            else if (engine.Equals(Engine.Slack))
            {
                Host.CreateDefaultBuilder().AddSlackHost();
            }
            else if (engine.Equals(Engine.AppInsights))
            {
                host = Host.CreateDefaultBuilder().AddAppInsightsHost();
            }
            else if (engine.Equals(Engine.Mock))
            {
                host = Host.CreateDefaultBuilder().AddMockHost();
            }
            else
                throw new NotSupportedEngineException(engine.Name);

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