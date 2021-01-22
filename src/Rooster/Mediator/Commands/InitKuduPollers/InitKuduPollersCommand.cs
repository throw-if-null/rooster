using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Hosting;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.StartKuduPoller;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.InitKuduPollers
{
    public class InitKuduPollersCommand : IOpinionatedRequestHandler<InitKuduPollersRequest, Unit>
    {
        private readonly Collection<PollerOptions> _options;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public InitKuduPollersCommand(
            IOptionsMonitor<Collection<PollerOptions>> options,
            IMediator mediator,
            ILogger<InitKuduPollersCommand> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Unit> Handle(InitKuduPollersRequest request, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>(_options.Count);

            foreach (var option in _options)
            {
                var startKuduPollerRequest = new StartKuduPollerRequest(_logger)
                {
                    KuduAdapters = option.KuduAdapters,
                    CurrentDateVarianceInSeconds = option.CurrentDateVarianceInSeconds,
                    PoolingIntervalInSeconds = option.PoolingIntervalInSeconds,
                    UseInternalPoller = option.UseInternalPoller
                };

                tasks.Add(_mediator.Send(startKuduPollerRequest, cancellationToken));
            }

            await Task.WhenAll(tasks);

            return Unit.Value;
        }
    }
}
