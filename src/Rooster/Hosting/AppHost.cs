using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
    public abstract class AppHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IHostApplicationLifetime _lifetime;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<AppHost> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _lifetime = lifetime;
            _kudus = kudus;
            _mediator = mediator;
            _logger = logger;

            _lifetime.ApplicationStarted.Register(() => { });
            _lifetime.ApplicationStopping.Register(() => { });
            _lifetime.ApplicationStopped.Register(() => { });
        }

        protected abstract string StartLogMessage { get; }

        protected abstract string StopLogMessage { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StartLogMessage, Array.Empty<object>());

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
            _logger.LogInformation(StopLogMessage, Array.Empty<object>());

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

            ProcessDockerLogsResponse response;

            try
            {
                response = await _mediator.Send(request, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{MethodName} failed.", nameof(ProcessDockerLogsCommand));

                response = new ProcessDockerLogsResponse
                {
                    Containers = containers
                };
            }


            if (!_options.UseInternalPoller)
                return;

            await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));

            await ProcessKuduLogs(kudu, response.Containers, ct);
        }
    }
}