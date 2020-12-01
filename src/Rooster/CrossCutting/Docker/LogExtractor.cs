using System;
using System.Globalization;

namespace Rooster.CrossCutting.Docker
{
    public static class LogExtractor
    {
        public static DockerCommandMetadata Extract(ReadOnlySpan<char> dockerCommandSpan)
        {
            var imageSpan = ExtractFullImageName(dockerCommandSpan);
            var (inbound, outbound) = ExtractPorts(dockerCommandSpan);

            return new DockerCommandMetadata
            {
                Date = ExtractDate(dockerCommandSpan),
                ServiceName = ExtractServiceName(dockerCommandSpan),
                ContainerName = ExtractContainerName(dockerCommandSpan),
                ImageName = ExtractImageName(imageSpan),
                ImageTag = ExtractImageTag(imageSpan),
                InboundPort = inbound,
                OutboundPort = outbound
            };
        }

        private static ReadOnlySpan<char> ExtractContainerName(ReadOnlySpan<char> span)
        {
            var nameSpan = "--name ".AsSpan();
            var nameIndex = span.IndexOf(nameSpan) + nameSpan.Length;
            var eIndex = span.IndexOf(" -e".AsSpan());

            var containerName = span[nameIndex..eIndex];

            return containerName;
        }

        private static DateTimeOffset ExtractDate(ReadOnlySpan<char> span)
        {
            var infoSpan = "INFO".AsSpan();
            var infoIndex = span.IndexOf(infoSpan);

            var date = span.Slice(0, infoIndex);

            return DateTimeOffset.Parse(date, null, DateTimeStyles.AssumeUniversal);
        }

        private static ReadOnlySpan<char> ExtractFullImageName(ReadOnlySpan<char> span)
        {
            var imageNameSpan = "DOCKER_CUSTOM_IMAGE_NAME=".AsSpan();
            var imageNameIndex = span.IndexOf(imageNameSpan) + imageNameSpan.Length;
            var eIndex = span[imageNameIndex..].IndexOf(" -e".AsSpan()) + imageNameIndex;

            var fullImageName = span[imageNameIndex..eIndex];

            return fullImageName;
        }

        private static ReadOnlySpan<char> ExtractImageName(ReadOnlySpan<char> fullImageName)
        {
            if (fullImageName.IsEmpty)
                return ReadOnlySpan<char>.Empty;

            var columnIndex = fullImageName.IndexOf(':');

            if (columnIndex == -1)
                return fullImageName;

            var name = fullImageName.Slice(0, columnIndex);

            return name;
        }

        private static ReadOnlySpan<char> ExtractImageTag(ReadOnlySpan<char> fullImageName)
        {
            if (fullImageName.IsEmpty)
                return ReadOnlySpan<char>.Empty;

            var columnIndex = fullImageName.IndexOf(':');

            if (columnIndex == -1)
                return ReadOnlySpan<char>.Empty;

            var tag = fullImageName[(columnIndex + 1)..];

            return tag;
        }

        private static (int inbound, int outbound) ExtractPorts(ReadOnlySpan<char> span)
        {
            var portsSpan = "-p".AsSpan();
            var dashSpan = "-".AsSpan();

            var portsIndex = span.IndexOf(portsSpan) + portsSpan.Length;
            var dashIndex = span[portsIndex..].IndexOf(dashSpan) + portsIndex;

            var ports = span[portsIndex..dashIndex];
            var columnIndex = ports.IndexOf(':');

            if (!int.TryParse(ports.Slice(0, columnIndex), out var inbound))
                inbound = -1;

            if (!int.TryParse(ports[(columnIndex + 1)..], out var outbound))
                outbound = -1;

            return (inbound, outbound);
        }

        private static ReadOnlySpan<char> ExtractServiceName(ReadOnlySpan<char> span)
        {
            var serviceNameSpan = "WEBSITE_SITE_NAME=".AsSpan();
            var serviceNameIndex = span.IndexOf(serviceNameSpan) + serviceNameSpan.Length;
            var eIndex = span[serviceNameIndex..].IndexOf(" -e".AsSpan()) + serviceNameIndex;

            var serviceName = span[serviceNameIndex..eIndex];

            return serviceName;
        }
    }
}
