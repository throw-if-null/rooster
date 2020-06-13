using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations
{
    public class MongoDbLogbookRepository : ILogbookRepository<ObjectId>
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

        public async Task Create(Logbook<ObjectId> logbook, CancellationToken cancellation)
        {
            _ = logbook ?? throw new ArgumentNullException(nameof(logbook));

            var collection = await _collectionFactory.Get<Logbook<ObjectId>>(cancellation);

            await collection.InsertOneAsync(logbook, GetInsertOneOptions(), cancellation);
        }

        public async Task<Logbook<ObjectId>> GetLast(CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<Logbook<ObjectId>>(cancellation);

            var filter = Builders<Logbook<ObjectId>>.Filter.Empty;
            var sort = Builders<Logbook<ObjectId>>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<Logbook<ObjectId>, Logbook<ObjectId>>
            {
                Sort = sort,
                Limit = 1
            });

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance;
        }
    }
}
