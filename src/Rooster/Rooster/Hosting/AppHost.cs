using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Handlers.ExportLogEntry;
using Rooster.Mediator.Handlers.ProcessLogEntry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public class AppHost<T> : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IKuduApiAdapter _kudu;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter kudu,
            IMediator mediator,
            ILogger<AppHost<T>> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var ct = cancellationToken;

            while (true)
            {
                var kuduLogs = await _kudu.GetDockerLogs(ct);

                foreach ((DateTimeOffset lastUpdated, Uri logUri, string machineName) in kuduLogs)
                {
                    if (lastUpdated < DateTimeOffset.UtcNow.AddMinutes(_options.CurrentDateVariance))
                        continue;

                    var lines = _kudu.ExtractLogsFromStream(logUri);

                    await foreach (var line in lines)
                    {
                        var exportedLogEntry = await _mediator.Send(new ExportLogEntryRequest { LogLine = line }, ct);

                        await _mediator.Send(new ProcessLogEntryRequest { ExportedLogEntry = exportedLogEntry }, ct);
                    }
                }

                if (!_options.UseInternalPoller)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));
            }

            _logger.LogDebug("Finished extracting docker logs.", null);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}