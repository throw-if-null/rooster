using MongoDB.Bson;
using System;

namespace Rooster.MongoDb.Schema
{
    public class LogEntry
    {
        public ObjectId Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public string ServiceName { get; set; }

        public string ContainerName { get; set; }

        public string ImageName { get; set; }

        public string ImageTag { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset EventDate { get; set; }
    }
}