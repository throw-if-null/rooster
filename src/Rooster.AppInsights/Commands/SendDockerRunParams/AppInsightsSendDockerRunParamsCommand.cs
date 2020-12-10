using MediatR;
using Rooster.AppInsights.Reporters;
using Rooster.Mediator.Commands.SendDockerRunParams;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppInsights.Commands.SendDockerRunParams
{
    public sealed class AppInsightsSendDockerRunParamsCommand : SendDockerRunParamsCommand
    {
        private readonly ITelemetryReporter _reporter;

        public AppInsightsSendDockerRunParamsCommand(ITelemetryReporter reporter)
        {
            _reporter = reporter;
        }

        protected override Task<Unit> SendImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation)
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(request.ContainerName)] = request.ContainerName,
                [nameof(request.Created)] = request.Created.ToString(),
                [nameof(request.EventDate)] = request.EventDate.ToString(),
                [nameof(request.ImageName)] = request.ImageName,
                [nameof(request.ImageTag)] = request.ImageTag,
                [nameof(request.InboundPort)] = request.InboundPort,
                [nameof(request.OutboundPort)] = request.OutboundPort,
                [nameof(request.ServiceName)] = request.ServiceName
            };

            _reporter.Report(properties);

            return Unit.Task;
        }
    }
}
