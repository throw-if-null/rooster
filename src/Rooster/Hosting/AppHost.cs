using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public class AppHost<T> : IHostedService
    {
        private readonly ConcurrentDictionary<string, long> _containers = new ConcurrentDictionary<string, long>();

        private readonly AppHostOptions _options;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<AppHost<T>> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudus = kudus ?? throw new ArgumentNullException(nameof(kudus));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var tasks = _kudus.Select(x => ProcessKuduLogs(x, cancellationToken));

            await Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessKuduLogs(IKuduApiAdapter kudu, CancellationToken ct)
        {
            var kuduLogs = await kudu.GetDockerLogs(ct);

            foreach ((DateTimeOffset lastUpdated, Uri logUri, string machineName) in kuduLogs.Where(x => x.LastUpdated.Date == DateTimeOffset.UtcNow.Date))
            {
                var extendedLastUpdated = lastUpdated.AddMinutes(_options.CurrentDateVarianceInMinutes);

                if (extendedLastUpdated < DateTimeOffset.UtcNow)
                {
                    _logger.LogDebug($"Log: {logUri} is old. Last updated: {lastUpdated}. Machine: {machineName}");

                    continue;
                }

                var lines = kudu.ExtractLogsFromStream(logUri);

                await foreach (var line in lines)
                {
                    var exportedLogEntry = await _mediator.Send(new ExportLogEntryRequest { LogLine = line }, ct);

                    if (exportedLogEntry.ContainerName.EndsWith("_msiproxy", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (_containers.ContainsKey(exportedLogEntry.ContainerName) &&
                        _containers[exportedLogEntry.ContainerName] <= exportedLogEntry.EventDate.Ticks)
                        continue;

                    await _mediator.Send(new ProcessLogEntryRequest { ExportedLogEntry = exportedLogEntry }, ct);

                    _containers[exportedLogEntry.ContainerName] = exportedLogEntry.EventDate.Ticks;
                }

                _logger.LogDebug($"Finished extracting docker logs from: {logUri}.", null);

                if (!_options.UseInternalPoller)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));
                await ProcessKuduLogs(kudu, ct);
            }
        }
    }
}