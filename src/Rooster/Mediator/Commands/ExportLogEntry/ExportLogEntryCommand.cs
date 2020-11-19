using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.CrossCutting.Docker;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ExportLogEntry
{
    public class ExportLogEntryCommand : IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>
    {
        private const string LogDockerLogLineReceived = "Received docker log line: {DockerLogLine}";

        private readonly ILogger _logger;

        public ExportLogEntryCommand(ILogger<ExportLogEntryCommand> logger)
        {
            _logger = logger;
        }

        public Task<ExportLogEntryResponse> Handle(ExportLogEntryRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug(LogDockerLogLineReceived, request.LogLine);

            var metadata = LogExtractor.Extract(request.LogLine);

            var logEntry = new ExportLogEntryResponse
            {
                ServiceName = metadata.ServiceName.ToString(),
                ContainerName = metadata.ContainerName.ToString(),
                ImageName = metadata.ImageName.ToString(),
                ImageTag = metadata.ImageTag.ToString(),
                InboundPort = metadata.InboundPort.ToString(),
                OutboundPort = metadata.OutboundPort.ToString(),
                EventDate = metadata.Date
            };

            return Task.FromResult(logEntry);
        }
    }
}