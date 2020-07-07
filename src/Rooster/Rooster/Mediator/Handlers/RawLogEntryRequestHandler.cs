using MediatR;
using Rooster.CrossCutting;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class RawLogEntryRequestHandler<T> : IRequestHandler<RawLogEntryRequest<T>, LogEntryRequest<T>>
    {
        private readonly ILogExtractor _extractor;

        public RawLogEntryRequestHandler(ILogExtractor extractor)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        public Task<LogEntryRequest<T>> Handle(RawLogEntryRequest<T> request, CancellationToken cancellationToken)
        {
            var (inboundPort, outboundPort) = _extractor.ExtractPorts(request.LogLine);

            var logEntry = new LogEntryRequest<T>
            {
                ImageName = _extractor.ExtractImageName(request.LogLine),
                ContainerName = _extractor.ExtractContainerName(request.LogLine),
                WebsiteName = _extractor.ExtractWebsiteName(request.LogLine),
                Date = _extractor.ExtractDate(request.LogLine),
                InboundPort = inboundPort,
                OutboundPort = outboundPort
            };

            return Task.FromResult(logEntry);
        }
    }
}
