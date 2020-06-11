using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Rooster.DataAccess.KuduInstances.Entities;
using System;

namespace Rooster.DataAccess.Logbooks.Entities
{
    public interface ILogbook
    {
    }

    public class Logbook<T, U> : ILogbook
        where U : IKuduInstance
    {
        [JsonIgnore] [BsonId]
        public T Id { get; set; }

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

        public U KuduInstance { get; set; }
    }
}