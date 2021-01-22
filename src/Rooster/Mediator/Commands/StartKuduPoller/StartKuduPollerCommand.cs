using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessAppLogSources;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public class StartKuduPollerCommand : IOpinionatedRequestHandler<StartKuduPollerRequest, Unit>
    {
        private readonly IMediator _mediator;
        private readonly KuduApiAdapterCache _cache;
        private readonly ILogger _logger;

        public StartKuduPollerCommand(IMediator mediator, KuduApiAdapterCache cache, ILogger<StartKuduPollerCommand> logger)
        {
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Unit> Handle(StartKuduPollerRequest request, CancellationToken cancellationToken)
        {
            var tasks =
                request.KuduAdapters.Select(name =>
                    _mediator.Send(
                        new ProcessAppLogSourcesRequest(_logger)
                        {
                            Kudu = _cache.Get(name),
                            CurrentDateVarianceInSeconds = request.CurrentDateVarianceInSeconds
                        },
                        cancellationToken)
                );

            await Task.WhenAll(tasks);

            if (!request.UseInternalPoller)
                return Unit.Value;

            await Task.Delay(TimeSpan.FromSeconds(request.PoolingIntervalInSeconds), cancellationToken);

            return await _mediator.Send(request, cancellationToken);
        }
    }
}
