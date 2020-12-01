using MediatR;
using Rooster.AppInsights.Reporters;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppInsights.Handlers.ProcessLogEntry
{
    public class AppInsightsProcessLogEntryCommand : AsyncRequestHandler<ShouldProcessDockerLogRequest>
    {
        private readonly ITelemetryReporter _reporter;

        public AppInsightsProcessLogEntryCommand(ITelemetryReporter reporter)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        protected override Task Handle(ShouldProcessDockerLogRequest request, CancellationToken cancellationToken)
        {
            var properties = new Dictionary<string, string>
            {
                [nameof(request.ExportedLogEntry.ContainerName)] = request.ExportedLogEntry.ContainerName,
                [nameof(request.ExportedLogEntry.Created)] = request.ExportedLogEntry.Created.ToString(),
                [nameof(request.ExportedLogEntry.EventDate)] = request.ExportedLogEntry.EventDate.ToString(),
                [nameof(request.ExportedLogEntry.ImageName)] = request.ExportedLogEntry.ImageName,
                [nameof(request.ExportedLogEntry.ImageTag)] = request.ExportedLogEntry.ImageTag,
                [nameof(request.ExportedLogEntry.InboundPort)] = request.ExportedLogEntry.InboundPort,
                [nameof(request.ExportedLogEntry.OutboundPort)] = request.ExportedLogEntry.OutboundPort,
                [nameof(request.ExportedLogEntry.ServiceName)] = request.ExportedLogEntry.ServiceName
            };

            _reporter.Report(properties);

            return Task.CompletedTask;
        }
    }
}