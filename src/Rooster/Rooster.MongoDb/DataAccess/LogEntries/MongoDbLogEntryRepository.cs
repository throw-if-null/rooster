using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.DataAccess.LogEntries
{
    public class MongoDbLogEntryRepository : LogEntryRepository<ObjectId>
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbLogEntryRepository(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override bool IsDefaultValue(ObjectId value)
        {
            return value == ObjectId.Empty;
        }

        protected override async Task CreateImplementation(LogEntry<ObjectId> entry, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<LogEntry<ObjectId>>(cancellation);

            await collection.InsertOneAsync(entry, GetInsertOneOptions(), cancellation);
        }

        protected override async Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(string serviceName, string containerName, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<LogEntry<ObjectId>>(cancellation);

            var filter = Builders<LogEntry<ObjectId>>.Filter.Where(x => x.ServiceName == serviceName&& x.ContainerName == containerName);
            var sort = Builders<LogEntry<ObjectId>>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<LogEntry<ObjectId>, LogEntry<ObjectId>>
            {
                Sort = sort,
                Limit = 1,
                Projection = Builders<LogEntry<ObjectId>>.Projection.Include(x => x.EventDate)
            });

            var entry = await cursor.FirstOrDefaultAsync();

            return entry == null ? default : entry.EventDate;
        }
    }
}