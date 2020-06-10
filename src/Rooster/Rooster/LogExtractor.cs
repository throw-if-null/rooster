using Rooster.DataAccess.AppServices.Entities;
using Rooster.DataAccess.LogEntries.Entities;
using System;

namespace Rooster
{
    public interface ILogExtractor
    {
        LogEntry Extract(string line);
    }

    public class LogExtractor : ILogExtractor
    {
        public LogEntry Extract(string line)
        {
            var (inbound, outbound) = ExtractPorts(line);
            var image = ExtractImageName(line);
            var website = ExtractWebsiteName(line);
            var host = ExtractHostName(line);
            var date = line.Remove(line.IndexOf("INFO") - 1);
            var name = ExtractContainerName(line);

            return new LogEntry
            {
                AppService = new SqlAppService { Name = website},
                ContainerName = name,
                HostName = host,
                ImageName = image,
                InboundPort = inbound,
                OutboundPort = outbound,
                Date = DateTimeOffset.Parse(date)
            };
        }

        private static (string inbound, string outbound) ExtractPorts(string line)
        {
            var portsValue = ExtractValue(line, "-p", "-");
            var ports = portsValue.Split(":");

            return (ports[0], ports[1]);
        }

        private static string ExtractImageName(string line)
        {
            return ExtractValue(line, "DOCKER_CUSTOM_IMAGE_NAME", "-e");
        }

        private static string ExtractWebsiteName(string line)
        {
            return ExtractValue(line, "WEBSITE_SITE_NAME", "-e");
        }

        private static string ExtractHostName(string line)
        {
            return ExtractValue(line, "WEBSITE_HOSTNAME", "-e");
        }

        private static string ExtractContainerName(string line)
        {
            return ExtractValue(line, "--name", "-e");
        }

        private static readonly Func<string, string, string, string> ExtractValue = delegate (string input, string key, string splitter)
        {
            var index = input.IndexOf(key);
            var value = input.Substring(index + key.Length + 1);
            value = value.Remove(value.IndexOf(splitter));

            return value;
        };
    }
}
