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
            while (true)
            {
                var kuduLogs = await _kudu.GetDockerLogs(cancellationToken);

                foreach ((DateTimeOffset LastUpdated, Uri LogUri, string MachineName) in kuduLogs)
                {
                    if (LastUpdated.Date < DateTimeOffset.UtcNow.Date)
                        continue;

                    var appServiceId =
                        await
                            _mediator.Send(
                                new AppServiceRequest<T> { KuduLogUri = LogUri },
                                cancellationToken);

                    var containerInstanceId =
                        await
                            _mediator.Send(
                                new ContainerInstanceRequest<T> { MachineName = MachineName, AppServiceId = appServiceId },
                                cancellationToken);

                    if (LastUpdated < DateTimeOffset.UtcNow.AddMinutes(_options.CurrentDateVariance))
                        return;

                    var lines = _kudu.ExtractLogsFromStream(LogUri);

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