using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessAppLogSources
{
    public class ProcessAppLogSourcesCommand : AsyncRequestHandler<ProcessAppLogSourcesRequest>
    {
        private const string ContinerProxySufix = "_msiproxy";
        private const string LogIsOldMessage = "Log: {0} is old. Last updated: {1}. Machine: {2}";
        private const string LogExtractionFinished = "Finished extracting docker logs from: {0}.";

        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ProcessAppLogSourcesCommand(ILogger<ProcessAppLogSourcesCommand> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        protected override async Task Handle(ProcessAppLogSourcesRequest request, CancellationToken cancellationToken)
        {
            var kuduLogs = await request.Kudu.GetDockerLogs(cancellationToken);
            var logs = kuduLogs.Where(x => x.LastUpdated.Date == DateTimeOffset.UtcNow.Date);

            var logsPerMachine = logs
                .GroupBy(x => x.MachineName)
                .Select(x => x.OrderByDescending(x => x.LastUpdated).First())
                .ToList();

            // TODO: Run in parallel

            foreach ((DateTimeOffset lastUpdated, Uri logUri, string machineName) in logsPerMachine)
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
                    ExtractDockerRunParamsResponse extractedParams = await _mediator.Send(new ExtractDockerRunParamsRequest { LogLine = line }, cancellationToken);

                    if (extractedParams.ContainerName.EndsWith(ContinerProxySufix, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    await _mediator.Send(new ShouldProcessDockerLogRequest { ExportedLogEntry = extractedParams }, cancellationToken);
                }

                _logger.LogDebug(LogExtractionFinished, logUri);
            }
        }
    }
}
