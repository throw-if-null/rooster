using Newtonsoft.Json;
using System;

namespace Rooster
{
    public class LogReference
    {
        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }
    }
}