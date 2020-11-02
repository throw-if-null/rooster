using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessKuduLogs
{
    public class ProcessKuduLogsCommand : AsyncRequestHandler<ProcessKuduLogsRequest>
    {
        private static readonly Func<ConcurrentDictionary<string, long>, ProcessDockerLogsResponse> CreateProcessDockerLogResponse =
            delegate (ConcurrentDictionary<string, long> containers)
            {
                return new ProcessDockerLogsResponse
                {
                    Containers = containers
                };
            };

        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ProcessKuduLogsCommand(IMediator mediator, ILogger<ProcessDockerLogsCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected override async Task Handle(ProcessKuduLogsRequest request, CancellationToken cancellationToken)
        {
            var processDockerLogsRequest = new ProcessDockerLogsRequest
            {
                Kudu = request.KuduAdapter,
                CurrentDateVarianceInMinutes = request.CurrentDateVarianceInMinutes,
                Containers = request.Containers
            };

            ProcessDockerLogsResponse response;

            try
            {
                response = await _mediator.Send(processDockerLogsRequest, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Command} failed.", nameof(ProcessDockerLogsCommand));

                response = new ProcessDockerLogsResponse
                {
                    Containers = request.Containers
                };
            }


            if (!request.UseInternalPoller)
                return;

            await Task.Delay(TimeSpan.FromSeconds(request.PoolingIntervalInSeconds));

            request.Containers = response.Containers;
            await _mediator.Send(request, cancellationToken);
        }
    }
}
