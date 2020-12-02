using System;

namespace Rooster.Mediator.Commands.Common
{
    public abstract record DockerRunParams
    {
        public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;

        public string ServiceName { get; init; }

        public string ContainerName { get; init; }

        public string ImageName { get; init; }

        public string ImageTag { get; init; }

        public string InboundPort { get; init; }

        public string OutboundPort { get; init; }

        public DateTimeOffset EventDate { get; init; }
    }
}
