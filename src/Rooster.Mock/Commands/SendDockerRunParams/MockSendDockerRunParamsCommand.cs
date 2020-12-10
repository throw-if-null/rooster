using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mock.Reporters;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mock.Commands.SendDockerRunParams
{
    public class MockSendDockerRunParamsCommand : SendDockerRunParamsCommand
    {
        private readonly IMockReporter _reporter;
        private readonly ILogger _logger;

        public MockSendDockerRunParamsCommand(IMockReporter reporter, ILogger<MockSendDockerRunParamsCommand> logger)
        {
            _reporter = reporter;
            _logger = logger;
        }

        protected override Task<Unit> SendImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation)
        {
            _logger.LogDebug("ProcessLogEntryRequest registered", Array.Empty<object>());

            _reporter.RegisterRequest(request);

            return Unit.Task;
        }
    }
}
