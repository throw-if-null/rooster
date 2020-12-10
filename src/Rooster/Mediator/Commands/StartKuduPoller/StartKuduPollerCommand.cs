using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public class StartKuduPollerCommand : IOpinionatedRequestHandler<StartKuduPollerRequest, Unit>
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public StartKuduPollerCommand(IMediator mediator, ILogger<StartKuduPollerCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(StartKuduPollerRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new ProcessAppLogSourcesRequest(_logger)
                {
                    Kudu = request.KuduAdapter,
                    CurrentDateVarianceInSeconds = request.CurrentDateVarianceInSeconds
                },
                cancellationToken);

            if (!request.UseInternalPoller)
                return Unit.Value;

            await Task.Delay(TimeSpan.FromSeconds(request.PoolingIntervalInSeconds), cancellationToken);

            return await _mediator.Send(request, cancellationToken);
        }
    }
}
