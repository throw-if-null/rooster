using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ShouldProcessDockerLog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessLogSource
{
    public class ProcessLogSourceCommand : AsyncRequestHandler<ProcessLogSourceRequest>
    {
        private const string ContinerProxySufix = "_msiproxy";
        private const string LogExtractionFinished = "Finished extracting docker logs from: {0}.";

        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ProcessLogSourceCommand(IMediator mediator, ILogger<ProcessLogSourceCommand> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        protected override async Task Handle(ProcessLogSourceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var lines = request.Kudu.ExtractLogsFromStream(request.LogUri);

                await foreach (var line in lines)
                {
                    ExtractDockerRunParamsResponse extractedParams =
                        await _mediator.Send(new ExtractDockerRunParamsRequest { LogLine = line }, cancellationToken);

                    if (extractedParams.ContainerName.EndsWith(ContinerProxySufix, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    await _mediator.Send(new ShouldProcessDockerLogRequest { ExportedLogEntry = extractedParams }, cancellationToken);
                }

                _logger.LogDebug(LogExtractionFinished, request.LogUri);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Command: {} failed.", nameof(ProcessLogSourceCommand));
            }
        }
    }
}
