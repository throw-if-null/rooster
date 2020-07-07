using MongoDB.Bson;
using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;

namespace Rooster.MongoDb.Handlers
{
    public class MongoDbLogEntryRequestHandler : LogEntryRequestHandler<ObjectId>
    {
        public MongoDbLogEntryRequestHandler(ILogEntryRepository<ObjectId> logEntryRepository)
            : base(logEntryRepository)
        {
        }
    }
}