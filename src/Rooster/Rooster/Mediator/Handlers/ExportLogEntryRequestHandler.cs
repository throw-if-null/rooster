using MediatR;
using Rooster.CrossCutting;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class ExportLogEntryRequestHandler<T> : IRequestHandler<ExportLogEntryRequest<T>, ProcessLogEntryRequest<T>>
    {
        private readonly ILogExtractor _extractor;

        protected ExportLogEntryRequestHandler(ILogExtractor extractor)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        public Task<ProcessLogEntryRequest<T>> Handle(ExportLogEntryRequest<T> request, CancellationToken cancellationToken)
        {
            var (inboundPort, outboundPort) = _extractor.ExtractPorts(request.LogLine);
            var (imageName, imageTag) = _extractor.ExtractImageName(request.LogLine);

            var logEntry = new ProcessLogEntryRequest<T>
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
