using Rooster.DataAccess.LogEntries.Entities;

namespace Rooster.DataAccess.LogEntries.Implementations.MongoDb
{
    public interface IMongoDbLogEntryRepository : ILogEntryRepository<MongoDbLogEntry>
    {
    }
}