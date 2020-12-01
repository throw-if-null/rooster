using System;

namespace Rooster.Test
{
    internal static class DockerRunParamsBuilder
    {
        private static readonly Random RandomGenerator = new Random();

        public static string BuildDockerLogLine(
            string containerName,
            string imageNameWithTag,
            string serviceName,
            string hostName,
            DateTimeOffset? logDate = null,
            string inboundPort = null,
            string outboundPort = null)
        {
            logDate = logDate ?? DateTimeOffset.UtcNow;

            var logLine =
                $"{logDate.Value.ToString("O")} INFO  - docker run -d " +
                $"{GeneratePorts(inboundPort, outboundPort)} " +
                $"{GenerateName(containerName)} " +
                $"{GenerateImageName(imageNameWithTag)} " +
                $"{GenerateWebSiteName(serviceName)} " +
                "-e WEBSITE_AUTH_ENABLED=False " +
                "-e WEBSITE_ROLE_INSTANCE_ID=0 " +
                $"{GenerateHostName(hostName)}" +
                "-e WEBSITE_INSTANCE_ID=dca0853ea787ff5bc1108ce74f2d26b3804e79ca665909dd8ae6df38a132d941 " +
                "-e HTTP_LOGGING_ENABLED=1 " +
                "appsvc/msitokenservice:2007200210";

            return logLine;
        }

        private static string GeneratePorts(string inboundPort, string outboundPort)
        {
            return $"-p {inboundPort}:{outboundPort}";
        }

        private static string GenerateName(string containerName)
        {
            return $"--name {containerName}";
        }

        private static string GenerateImageName(string imageNameWithTag)
        {
            return $"-e DOCKER_CUSTOM_IMAGE_NAME={imageNameWithTag}";
        }

        private static string GenerateWebSiteName(string serviceName)
        {
            return $"-e WEBSITE_SITE_NAME={serviceName}";
        }

        private static string GenerateHostName(string hostName)
        {
            return $"-e WEBSITE_HOSTNAME={hostName}";
        }
    }
}
