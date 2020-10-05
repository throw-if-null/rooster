using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.CrossCutting.Docker;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ExportLogEntry
{
    public class ExportLogEntryCommand : IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>
    {
        private const string LogDockerLogLineReceived = "Received docker log line: {DockerLogLine}";

        private readonly ILogExtractor _extractor;
        private readonly ILogger _logger;

        public ExportLogEntryCommand(ILogExtractor extractor, ILogger<ExportLogEntryCommand> logger)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ExportLogEntryResponse> Handle(ExportLogEntryRequest request, CancellationToken cancellationToken)
        {
            _logger.LogDebug(LogDockerLogLineReceived, request.LogLine);

            var (inboundPort, outboundPort) = _extractor.ExtractPorts(request.LogLine);
            var (imageName, imageTag) = _extractor.ExtractImageName(request.LogLine);

            var logEntry = new ExportLogEntryResponse
            {
                ServiceName = _extractor.ExtractServiceName(request.LogLine),
                ContainerName = _extractor.ExtractContainerName(request.LogLine),
                ImageName = imageName,
                ImageTag = imageTag,
                InboundPort = inboundPort,
                OutboundPort = outboundPort,
                EventDate = _extractor.ExtractDate(request.LogLine)
            };

            return Task.FromResult(logEntry);
        }
    }
}