using System;

namespace Benchmarks
{
    public class LogExtractorOld
    {
        private const string Dash = "-";
        private const char Column = ':';
        private const string PortsPrefix = "-p";
        private const string EnvironmentVariablePrefix = " -e";
        private const string ImageKey = "DOCKER_CUSTOM_IMAGE_NAME";
        private const string ServiceNameKey = "WEBSITE_SITE_NAME";
        private const string ContainerNameKey = "--name";
        private const string DateKey = "INFO";

        private static readonly Func<string, string, string, string> ExtractValue = delegate (string input, string key, string splitter)
        {
            var index = input.IndexOf(key);
            var value = input.Substring(index + key.Length + 1);
            value = value.Remove(value.IndexOf(splitter));

            return value.Trim().ToLowerInvariant();
        };

        public (string inbound, string outbound) ExtractPorts(string line)
        {
            var portsValue = ExtractValue(line, PortsPrefix, Dash);
            var ports = portsValue.Split(Column);

            return (ports[0], ports[1]);
        }

        public (string name, string tag) ExtractImageName(string line)
        {
            var imageWithTag = ExtractValue(line, ImageKey, EnvironmentVariablePrefix);
            var parts = imageWithTag.Split(Column);

            return (parts[0], parts[1]);
        }

        public string ExtractServiceName(string line)
        {
            return ExtractValue(line, ServiceNameKey, EnvironmentVariablePrefix);
        }

        public string ExtractContainerName(string line)
        {
            return ExtractValue(line, ContainerNameKey, EnvironmentVariablePrefix);
        }

        public DateTimeOffset ExtractDate(string line)
        {
            var date = line.Remove(line.IndexOf(DateKey) - 1);
            DateTimeOffset.TryParse(date, out var convertedDate);

            return convertedDate;
        }
    }
}