using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances.Implementations.MongoDb
{
    public class MongoDbKuduInstanceRepository : IMongoDbKuduInstanceRepository
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly IKuduInstanceCollectionFactory _collectionFactory;

        public MongoDbKuduInstanceRepository(IKuduInstanceCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        public async Task<MongoDbKuduInstance> Create(MongoDbKuduInstance kuduInstance, CancellationToken cancellation)
        {
            _ = kuduInstance ?? throw new ArgumentNullException(nameof(kuduInstance));

            var collection = await _collectionFactory.Get<MongoDbKuduInstance>(cancellation);

            await collection.InsertOneAsync(kuduInstance, GetInsertOneOptions(), cancellation);

            return kuduInstance;
        }

        public async Task<MongoDbKuduInstance> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            var trimmedName = name.Trim();
            var collection = await _collectionFactory.Get<MongoDbKuduInstance>(cancellation);

            var cursor = await collection.FindAsync(x => x.Name == trimmedName, null, cancellation);

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance;
        }

        public async Task<string> GetNameById(string id, CancellationToken cancellation)
        {
            if (ObjectId.TryParse(id, out var objectId))
                return default;

            var collection = await _collectionFactory.Get<MongoDbKuduInstance>(cancellation);

            var cursor = await collection.FindAsync(x => x.Id == objectId, null, cancellation);

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance.Name;
        }
    }
}