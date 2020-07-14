using MediatR;
using System;

namespace Rooster.Mediator.Requests
{
    public class ProcessLogEntryRequest<T> : IRequest
    {
        public T Id { get; set; }

        public string ServiceName { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public string ImageName { get; set; }

        public string ImageTag { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset EventDate { get; set; }
    }
}