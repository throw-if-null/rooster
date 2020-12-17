using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.StartKuduPoller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.InitKuduPollers
{
    public class InitKuduPollersCommand : IOpinionatedRequestHandler<InitKuduPollersRequest, Unit>
    {
        private readonly AppHostOptions _options;
        private readonly IEnumerable<IKuduApiAdapter> _kudus;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public InitKuduPollersCommand(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<InitKuduPollersCommand> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudus = kudus;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(InitKuduPollersRequest request, CancellationToken cancellationToken)
        {
            var kudus = _kudus.ToArray();
            var tasks = new Task[kudus.Length];

            for (var i = 0; i < kudus.Length; i++)
            {
                var startKuduPollerRequest = new StartKuduPollerRequest(_logger)
                {
                    KuduAdapter = kudus[i],
                    CurrentDateVarianceInSeconds = _options.CurrentDateVarianceInSeconds,
                    PoolingIntervalInSeconds = _options.PoolingIntervalInSeconds,
                    UseInternalPoller = _options.UseInternalPoller
                };

                tasks[i] = _mediator.Send(startKuduPollerRequest, cancellationToken);
            }

            await Task.WhenAll(tasks);

            return Unit.Value;
        }
    }
}
