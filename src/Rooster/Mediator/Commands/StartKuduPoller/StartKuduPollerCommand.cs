using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public class StartKuduPollerCommand : AsyncRequestHandler<StartKuduPollerRequest>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public StartKuduPollerCommand(IMediator mediator, ILogger<StartKuduPollerCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected override async Task Handle(StartKuduPollerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.Send(
                    new ProcessAppLogSourcesRequest
                    {
                        Kudu = request.KuduAdapter,
                        CurrentDateVarianceInMinutes = request.CurrentDateVarianceInMinutes
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{Command} failed.", nameof(ProcessAppLogSourcesCommand));
            }

            if (!request.UseInternalPoller)
                return;

            await Task.Delay(TimeSpan.FromSeconds(request.PoolingIntervalInSeconds), cancellationToken);

            await _mediator.Send(request, cancellationToken);
        }
    }
}
