using MediatR;
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

        public MockProcessLogEntryCommand(IMockReporter reporter)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        protected override Task Handle(ProcessLogEntryRequest request, CancellationToken cancellationToken)
        {
            _reporter.RegisterRequest(request);

            return Task.CompletedTask;
        }
    }
}
