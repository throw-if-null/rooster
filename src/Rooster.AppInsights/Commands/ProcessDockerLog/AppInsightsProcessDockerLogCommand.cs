using MediatR;
using Rooster.AppInsights.Reporters;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessDockerLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppInsights.Handlers.ProcessDockerLog
{
    public class AppInsightsProcessDockerLogCommand : ProcessDockerLogCommand
    {
        public AppInsightsProcessDockerLogCommand(IMediator mediator) : base(mediator)
        {
        }

        protected override Task<bool> ShouldProcessDockerLog(DockerRunParams parameters, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }
}