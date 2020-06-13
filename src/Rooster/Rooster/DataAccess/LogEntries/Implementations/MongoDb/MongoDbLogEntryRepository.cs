using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries.Implementations.MongoDb
{
    public class MongoDbLogEntryRepository : IMongoDbLogEntryRepository
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

        public async Task Create(MongoDbLogEntry entry, CancellationToken cancellation)
        {
            _ = entry ?? throw new ArgumentNullException(nameof(entry));

            var collection = await _collectionFactory.Get<MongoDbLogEntry>(cancellation);

            await collection.InsertOneAsync(entry, GetInsertOneOptions(), cancellation);
        }

        public async Task<DateTimeOffset> GetLatestForAppService(string appServiceId, CancellationToken cancellation)
        {
            if (ObjectId.TryParse(appServiceId, out var objectId))
                return default;

            var collection = await _collectionFactory.Get<MongoDbLogEntry>(cancellation);

            var filter = Builders<MongoDbLogEntry>.Filter.Where(x => x.AppServiceId == objectId);
            var sort = Builders<MongoDbLogEntry>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<MongoDbLogEntry, MongoDbLogEntry>
            {
                Sort = sort,
                Limit = 1
            });

            var entry = await cursor.FirstOrDefaultAsync();

            return entry.Date;
        }
    }
}