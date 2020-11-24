using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ProcessAppLogSource;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public class StartKuduPollerCommand : AsyncRequestHandler<StartKuduPollerRequest>
    {
        private static readonly Func<ConcurrentDictionary<string, long>, ProcessAppLogSourceResponse> CreateProcessDockerLogResponse =
            delegate (ConcurrentDictionary<string, long> containers)
            {
                return new ProcessAppLogSourceResponse
                {
                    Containers = containers
                };
            };

        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public StartKuduPollerCommand(IMediator mediator, ILogger<ProcessAppLogSourceCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected override async Task Handle(StartKuduPollerRequest request, CancellationToken cancellationToken)
        {
            ProcessAppLogSourceResponse response;

            try
            {
                response = await _mediator.Send(
                    new ProcessAppLogSourceRequest
                    {
                        Kudu = request.KuduAdapter,
                        CurrentDateVarianceInMinutes = request.CurrentDateVarianceInMinutes,
                        Containers = request.Containers
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Command} failed.", nameof(ProcessAppLogSourceCommand));

                response = new ProcessAppLogSourceResponse
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
