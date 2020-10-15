using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mock.Reporters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mock.Commands
{
    public class MockProcessLogEntryCommand : AsyncRequestHandler<ProcessLogEntryRequest>
    {
        private readonly IMockReporter _reporter;
        private readonly ILogger _logger;

        public MockProcessLogEntryCommand(IMockReporter reporter, ILogger<MockProcessLogEntryCommand> logger)
        {
            _reporter = reporter;
            _logger = logger;
        }

        protected override Task Handle(ProcessLogEntryRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("ProcessLogEntryRequest registered", Array.Empty<object>());

            _reporter.RegisterRequest(request);

            return Task.CompletedTask;
        }
    }
}
