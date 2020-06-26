using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.DataAccess.Logbooks
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

        protected override async Task<DateTimeOffset> GetLatestDateForKuduInstanceImplementation(ObjectId kuduInstanceId, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<Logbook<ObjectId>>(cancellation);

            var filter = Builders<Logbook<ObjectId>>.Filter.Where(x => x.KuduInstanceId == kuduInstanceId);
            var sort = Builders<Logbook<ObjectId>>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<Logbook<ObjectId>, Logbook<ObjectId>>
            {
                Sort = sort,
                Limit = 1,
                Projection = Builders<Logbook<ObjectId>>.Projection.Include(x => x.LastUpdated)
            });

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance == null ? default : kuduInstance.LastUpdated;
        }
    }
}
