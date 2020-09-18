using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public class AppHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudus = kudus ?? throw new ArgumentNullException(nameof(kudus));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var kudus = _kudus.ToArray();
            var tasks = new Task[_kudus.Count()];

            for (var i = 0; i < _kudus.Count(); i++)
            {
                tasks[i] = ProcessKuduLogs(kudus[i], new ConcurrentDictionary<string, long>(), cancellationToken);
            }

            await Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ProcessKuduLogs(IKuduApiAdapter kudu, ConcurrentDictionary<string ,long> containers, CancellationToken ct)
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