using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessDockerLog;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mock.Commands.ProcessLogEntry
{
    public class MockProcessDockerLogCommand : ProcessDockerLogCommand
    {
        // TODO: Track the latest processed request date so we don't process it again!!!
        public MockProcessDockerLogCommand(IMediator mediator) : base(mediator)
        {
        }

        protected override Task<bool> ShouldProcessDockerLog(DockerRunParams parameters, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}
