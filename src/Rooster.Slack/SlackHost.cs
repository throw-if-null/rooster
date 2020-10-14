using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack
{
    public class SlackHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public SlackHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<SlackHost> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudus = kudus ?? throw new ArgumentNullException(nameof(kudus));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{nameof(SlackHost)} started.", Array.Empty<object>());

            try
            {
                var kudus = _kudus.ToArray();
                var tasks = new Task[_kudus.Count()];

                for (var i = 0; i < _kudus.Count(); i++)
                {
                    tasks[i] = ProcessKuduLogs(kudus[i], new ConcurrentDictionary<string, long>(), cancellationToken);
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(SlackHost)} failed.", Array.Empty<object>());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessKuduLogs(IKuduApiAdapter kudu, ConcurrentDictionary<string, long> containers, CancellationToken ct)
        {
            var request = new ProcessDockerLogsRequest
            {
                Kudu = kudu,
                CurrentDateVarianceInMinutes = _options.CurrentDateVarianceInMinutes,
                Containers = containers
            };

            var response = await _mediator.Send(request, ct);

            if (!_options.UseInternalPoller)
                return;

            await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));

            await ProcessKuduLogs(kudu, response.Containers, ct);
        }
    }
}