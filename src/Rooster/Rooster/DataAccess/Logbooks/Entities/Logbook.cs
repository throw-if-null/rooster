using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace Rooster.DataAccess.Logbooks.Entities
{
    public class Logbook<T>
    {
        [JsonIgnore] [BsonId]
        public T Id { get; set; }

        [JsonIgnore]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        [JsonProperty("machineName")]
        public string MachineName { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("size")] [BsonIgnore]
        public int Size { get; set; }

        [JsonProperty("href")] [BsonIgnore]
        public Uri Href { get; set; }

        [JsonProperty("path")] [BsonIgnore]
        public string Path { get; set; }

        public T KuduInstanceId { get; set; }
    }
}