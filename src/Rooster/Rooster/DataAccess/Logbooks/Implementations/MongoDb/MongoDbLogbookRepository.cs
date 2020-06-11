using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations.MongoDb
{
    public class MongoDbLogbookRepository : IMongoDbLogbookRepository
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly ILogbooCollectionFactory _collectionFactory;

        public MongoDbLogbookRepository(ILogbooCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        public async Task Create(MongoDbLogbook logbook, CancellationToken cancellation)
        {
            _ = logbook ?? throw new ArgumentNullException(nameof(logbook));

            var collection = await _collectionFactory.Get<MongoDbLogbook>(cancellation);

            await collection.InsertOneAsync(logbook, GetInsertOneOptions(), cancellation);
        }

        public async Task<MongoDbLogbook> GetLast(CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<MongoDbLogbook>(cancellation);

            var filter = Builders<MongoDbLogbook>.Filter.Empty;
            var sort = Builders<MongoDbLogbook>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<MongoDbLogbook, MongoDbLogbook>
            {
                Sort = sort,
                Limit = 1
            });

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance;
        }
    }
}
