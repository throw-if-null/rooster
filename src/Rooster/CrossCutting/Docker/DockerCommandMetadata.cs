using System;

namespace Rooster.CrossCutting.Docker
{
    public ref struct DockerCommandMetadata
    {
        public ReadOnlySpan<char> ImageName { get; set; }

        public ReadOnlySpan<char> ImageTag { get; set; }

        public ReadOnlySpan<char> ServiceName { get; set; }

        public ReadOnlySpan<char> ContainerName { get; set; }

        public DateTimeOffset Date { get; set; }

        public int InboundPort { get; set; }

        public int OutboundPort { get; set; }
    }
}
