using MongoDB.Bson;
using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;

namespace Rooster.MongoDb.Handlers
{
    public class MongoDbProcessLogEntryRequestHandler : ProcessLogEntryRequestHandler<ObjectId>
    {
        public MongoDbProcessLogEntryRequestHandler(ILogEntryRepository<ObjectId> logEntryRepository)
            : base(logEntryRepository)
        {
        }
    }
}