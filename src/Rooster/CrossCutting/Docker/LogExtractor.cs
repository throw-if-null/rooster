using System;

namespace Rooster.CrossCutting.Docker
{
    public interface ILogExtractor
    {
        (string inbound, string outbound) ExtractPorts(string line);

        (string name, string tag) ExtractImageName(string line);

        string ExtractServiceName(string line);

        string ExtractContainerName(string line);

        DateTimeOffset ExtractDate(string line);
    }

    public class LogExtractor : ILogExtractor
    {
        private static readonly Func<string, string, string, string> ExtractValue = delegate (string input, string key, string splitter)
        {
            var index = input.IndexOf(key);
            var value = input.Substring(index + key.Length + 1);
            value = value.Remove(value.IndexOf(splitter));

            return value.Trim().ToLowerInvariant();
        };

        public (string inbound, string outbound) ExtractPorts(string line)
        {
            var portsValue = ExtractValue(line, "-p", "-");
            var ports = portsValue.Split(":");

            return (ports[0], ports[1]);
        }

        public (string name, string tag) ExtractImageName(string line)
        {
            var imageWithTag = ExtractValue(line, "DOCKER_CUSTOM_IMAGE_NAME", "-e");
            var parts = imageWithTag.Split(":");

            return (parts[0], parts[1]);
        }

        public string ExtractServiceName(string line)
        {
            return ExtractValue(line, "WEBSITE_SITE_NAME", "-e");
        }

        public string ExtractContainerName(string line)
        {
            return ExtractValue(line, "--name", "-e");
        }

        public DateTimeOffset ExtractDate(string line)
        {
            var date = line.Remove(line.IndexOf("INFO") - 1);
            DateTimeOffset.TryParse(date, out var convertedDate);

            return convertedDate;
        }
    }
}