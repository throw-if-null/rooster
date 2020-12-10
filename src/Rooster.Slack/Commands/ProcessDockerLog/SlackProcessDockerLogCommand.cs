using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessDockerLog;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Commands.ProcessDockerLog
{
    public class SlackProcessDockerLogCommand : ProcessDockerLogCommand
    {
        public SlackProcessDockerLogCommand(IMediator mediator) : base(mediator)
        {
        }

        protected override Task<bool> ShouldProcessDockerLog(DockerRunParams parameters, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}