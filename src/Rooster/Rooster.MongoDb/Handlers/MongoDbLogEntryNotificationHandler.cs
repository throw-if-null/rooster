using MongoDB.Bson;
using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.Handlers;

namespace Rooster.MongoDb.Handlers
{
    public class MongoDbLogEntryNotificationHandler : LogEntryNotificationHandler<ObjectId>
    {
        public MongoDbLogEntryNotificationHandler(
            ILogEntryRepository<ObjectId> logEntryRepository,
            ILogExtractor extractor)
            : base(logEntryRepository, extractor)
        {
        }
    }
}