using Newtonsoft.Json;
using System;

namespace Rooster.DataAccess.Logbooks.Entities
{
    public class Logbook
    {
        [JsonIgnore]
        public int Id { get; set; }

        [JsonIgnore]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("href")]
        public Uri Href { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        public int KuduInstanceId { get; set; }
    }
}