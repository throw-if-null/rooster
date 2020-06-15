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

        private readonly ILogbookCollectionFactory _collectionFactory;

        public MongoDbLogbookRepository(ILogbookCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        public async Task Create(Logbook<ObjectId> logbook, CancellationToken cancellation)
        {
            ValidateLogbook(logbook);

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

        private static void ValidateLogbook(Logbook<ObjectId> logbook)
        {
            _ = logbook ?? throw new ArgumentNullException(nameof(logbook));

            if (logbook.KuduInstanceId == ObjectId.Empty)
                ThrowArgumentException(nameof(logbook.KuduInstanceId), logbook.KuduInstanceId.ToString());

            if (logbook.LastUpdated == default || logbook.LastUpdated == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logbook.LastUpdated), logbook.LastUpdated.ToString());

            if (string.IsNullOrWhiteSpace(logbook.MachineName))
                ThrowArgumentException(nameof(logbook.MachineName), logbook.MachineName == null ? "NULL" : "EMPTY");
        }

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };
    }
}
