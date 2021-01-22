using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.InitKuduPollers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public abstract class PollerHost : IHostedService
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public PollerHost(
            IMediator mediator,
            ILogger<PollerHost> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected abstract string StartLogMessage { get; }

        protected abstract string StopLogMessage { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StartLogMessage, Array.Empty<object>());

            await _mediator.Send(new InitKuduPollersRequest(), cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(StopLogMessage, Array.Empty<object>());

            return Task.CompletedTask;
        }
    }
}