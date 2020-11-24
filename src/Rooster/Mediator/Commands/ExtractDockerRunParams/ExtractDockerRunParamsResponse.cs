using System;

namespace Rooster.Mediator.Commands.ExtractDockerRunParams
{
    public class ExtractDockerRunParamsResponse
    {
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
