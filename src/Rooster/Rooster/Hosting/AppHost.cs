using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public class AppHost<T> : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IKuduApiAdapter<T> _kudu;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter<T> kudu,
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
            while (true)
            {
                var logbooks = await _kudu.GetDockerLogs(cancellationToken);

                foreach (var logbook in logbooks)
                {
                    if (logbook.LastUpdated.Date < DateTimeOffset.UtcNow.Date)
                        continue;

                    var appServiceId =
                        await
                            _mediator.Send(
                                new AppServiceRequest<T> { KuduLogUri = logbook.Href },
                                cancellationToken);

                    var containerInstanceId =
                        await
                            _mediator.Send(
                                new ContainerInstanceRequest<T> { MachineName = logbook.MachineName, AppServiceId = appServiceId },
                                cancellationToken);

                    var lastUpdateDate =
                        await
                            _mediator.Send(
                                new LogbookRequest<T>
                                {
                                    ContainerInstanceId = containerInstanceId,
                                    LastUpdated = logbook.LastUpdated,
                                    MachineName = logbook.MachineName
                                },
                                cancellationToken);

                    if (logbook.LastUpdated < lastUpdateDate)
                        return;

                    var lines = _kudu.ExtractLogsFromStream(logbook);

                    await foreach (var line in lines)
                    {
                        LogEntryRequest<T> logEntryRequest = await _mediator.Send(new RawLogEntryRequest<T> { LogLine = line }, cancellationToken);

                        await _mediator.Send(logEntryRequest, cancellationToken);
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