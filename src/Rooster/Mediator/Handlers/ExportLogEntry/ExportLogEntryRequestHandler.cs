using MediatR;
using Rooster.CrossCutting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers.ExportLogEntry
{
    public class ExportLogEntryRequestHandler : IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>
    {
        private readonly ILogExtractor _extractor;

        public ExportLogEntryRequestHandler(ILogExtractor extractor)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        public Task<ExportLogEntryResponse> Handle(ExportLogEntryRequest request, CancellationToken cancellationToken)
        {
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