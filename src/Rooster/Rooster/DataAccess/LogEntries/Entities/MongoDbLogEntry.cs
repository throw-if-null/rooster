using MongoDB.Bson;
using System;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class MongoDbLogEntry : LogEntry<ObjectId>
    {
        public MongoDbLogEntry(
            ObjectId appserviceId,
            string hostName,
            string imageName,
            string containerName,
            string inboundPort,
            string outboundPort,
            DateTimeOffset date)
            : base(appserviceId, hostName, imageName, containerName, inboundPort, outboundPort, date)
        {
        }

        protected override bool ValidateT(ObjectId value)
        {
            return value != null;
        }
    }
}