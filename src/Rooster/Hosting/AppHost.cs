using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.ProcessKuduLogs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public abstract class AppHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<AppHost> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudus = kudus;
            _mediator = mediator;
            _logger = logger;
        }

        protected abstract string StartLogMessage { get; }

        protected abstract string StopLogMessage { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StartLogMessage, Array.Empty<object>());

            var kudus = _kudus.ToArray();
            var tasks = new Task[kudus.Length];

            for (var i = 0; i < kudus.Length; i++)
            {
                tasks[i] = _mediator.Send(new ProcessKuduLogsRequest
                {
                    KuduAdapter = kudus[i],
                    Containers = new ConcurrentDictionary<string, long>(),
                    CurrentDateVarianceInMinutes = _options.CurrentDateVarianceInMinutes,
                    PoolingIntervalInSeconds = _options.PoolingIntervalInSeconds,
                    UseInternalPoller = _options.UseInternalPoller
                });
            }

            await Task.WhenAll(tasks);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StopLogMessage, Array.Empty<object>());

            return Task.CompletedTask;
        }
    }
}