using System;

namespace Rooster.Mediator.Commands.ExtractDockerRunParams
{
    public record ExtractDockerRunParamsResponse
    {
        public string ServiceName { get; init; }

        public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;

        public string ImageName { get; init; }

        public string ImageTag { get; init; }

        public string ContainerName { get; init; }

        public string InboundPort { get; init; }

        public string OutboundPort { get; init; }

        public DateTimeOffset EventDate { get; init; }
    }
}
