﻿using System;

namespace Rooster.CrossCutting
{
    public interface ILogExtractor
    {
        (string inbound, string outbound) ExtractPorts(string line);

        string ExtractImageName(string line);

        string ExtractWebsiteName(string line);

        string ExtractContainerName(string line);

        DateTimeOffset ExtractDate(string line);
    }

    public class LogExtractor : ILogExtractor
    {
        public (string inbound, string outbound) ExtractPorts(string line)
        {
            var portsValue = ExtractValue(line, "-p", "-");
            var ports = portsValue.Split(":");

            return (ports[0], ports[1]);
        }

        public string ExtractImageName(string line)
        {
            return ExtractValue(line, "DOCKER_CUSTOM_IMAGE_NAME", "-e");
        }

        public string ExtractWebsiteName(string line)
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

        private static readonly Func<string, string, string, string> ExtractValue = delegate (string input, string key, string splitter)
        {
            var index = input.IndexOf(key);
            var value = input.Substring(index + key.Length + 1);
            value = value.Remove(value.IndexOf(splitter));

            return value;
        };
    }
}