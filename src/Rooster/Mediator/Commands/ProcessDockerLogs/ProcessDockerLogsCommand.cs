using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessDockerLogs
{
    public class ProcessDockerLogsCommand : IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>
    {
        private const string ContinerProxySufix = "_msiproxy";
        private const string LogIsOldMessage = "Log: {0} is old. Last updated: {1}. Machine: {2}";
        private const string LogExtractionFinished = "Finished extracting docker logs from: {0}.";

        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ProcessDockerLogsCommand(ILogger<ProcessDockerLogsCommand> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<ProcessDockerLogsResponse> Handle(ProcessDockerLogsRequest request, CancellationToken cancellationToken)
        {
            var kuduLogs = await request.Kudu.GetDockerLogs(cancellationToken);
            var logs = kuduLogs.Where(x => x.LastUpdated.Date == DateTimeOffset.UtcNow.Date);

            foreach ((DateTimeOffset lastUpdated, Uri logUri, string machineName) in logs)
            {
                var lastUpdatedEx = lastUpdated.AddMinutes(request.CurrentDateVarianceInMinutes);

                if (lastUpdatedEx < DateTimeOffset.UtcNow)
                {
                    _logger.LogDebug(LogIsOldMessage, logUri, lastUpdated, machineName);

                    continue;
                }

                var lines = request.Kudu.ExtractLogsFromStream(logUri);

                await foreach (var line in lines)
                {
                    var exportedLogEntry = await _mediator.Send(new ExportLogEntryRequest { LogLine = line }, cancellationToken);

                    if (exportedLogEntry.ContainerName.EndsWith(ContinerProxySufix, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (request.Containers.ContainsKey(exportedLogEntry.ContainerName) &&
                        request.Containers[exportedLogEntry.ContainerName] <= exportedLogEntry.EventDate.Ticks)
                        continue;

                    await _mediator.Send(new ProcessLogEntryRequest { ExportedLogEntry = exportedLogEntry }, cancellationToken);

                    request.Containers[exportedLogEntry.ContainerName] = exportedLogEntry.EventDate.Ticks;
                }

                _logger.LogDebug(LogExtractionFinished, logUri);
            }

            return new ProcessDockerLogsResponse { Containers = request.Containers };
        }
    }
}
