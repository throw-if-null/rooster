using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations
{
    public class MongoDbLogbookRepository : LogbookRepository<ObjectId>
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly ILogbookCollectionFactory _collectionFactory;

        public MongoDbLogbookRepository(ILogbookCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override bool IsDefaultValue(ObjectId value)
        {
            return value == ObjectId.Empty;
        }

        protected override async Task CreateImplementation(Logbook<ObjectId> logbook, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<Logbook<ObjectId>>(cancellation);

            await collection.InsertOneAsync(logbook, GetInsertOneOptions(), cancellation);
        }

        public override async Task<Logbook<ObjectId>> GetLast(CancellationToken cancellation)
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
