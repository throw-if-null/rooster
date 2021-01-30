using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Rooster.App;
using Rooster.CrossCutting;
using Rooster.HealthCheck;
using Rooster.Hosting;
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
        var engines = Configuration.Value.GetSection($"{nameof(PollerOptions)}").Get<Collection<PollerOptions>>();
        var hosts = new List<IHost>() { Host.CreateDefaultBuilder().ConfigureHealthCheck() };

        foreach (var engine in Engine.ToList(engines?.Select(e => e.Engine)))
        {
            var host = EngineHostBuilder.ResolveAndBuild(engine);

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