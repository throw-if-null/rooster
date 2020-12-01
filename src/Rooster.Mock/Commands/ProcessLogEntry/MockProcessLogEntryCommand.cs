using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using Rooster.Mock.Reporters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mock.Commands.ProcessLogEntry
{
    public class MockProcessLogEntryCommand : AsyncRequestHandler<ShouldProcessDockerLogRequest>
    {
        private readonly IMockReporter _reporter;
        private readonly ILogger _logger;

        public MockProcessLogEntryCommand(IMockReporter reporter, ILogger<MockProcessLogEntryCommand> logger)
        {
            _reporter = reporter;
            _logger = logger;
        }

        protected override Task Handle(ShouldProcessDockerLogRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("ProcessLogEntryRequest registered", Array.Empty<object>());

            _reporter.RegisterRequest(request);

            return Task.CompletedTask;
        }
    }
}
