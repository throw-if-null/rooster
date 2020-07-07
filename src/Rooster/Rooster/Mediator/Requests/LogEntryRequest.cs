using MediatR;
using System;

namespace Rooster.Mediator.Requests
{
    public class LogEntryRequest<T> : IRequest
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public T LogbookId { get; set; }

        public string ImageName { get; set; }

        public string WebsiteName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset Date { get; set; }
    }
}