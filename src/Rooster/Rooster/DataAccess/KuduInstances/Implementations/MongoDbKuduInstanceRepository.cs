using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances.Implementations
{
    public class MongoDbKuduInstanceRepository : KuduInstanceRepository<ObjectId>
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

        protected override bool IsDefaultValue(ObjectId value)
        {
            return value == ObjectId.Empty;
        }

        protected override async Task<ObjectId> CreateImplementation(KuduInstance<ObjectId> kuduInstance, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<KuduInstance<ObjectId>>(cancellation);

            await collection.InsertOneAsync(kuduInstance, GetInsertOneOptions(), cancellation);

            return kuduInstance.Id;
        }

        protected override async Task<ObjectId> GetIdByNameImplementation(string name, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<KuduInstance<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Name == name,
                new FindOptions<KuduInstance<ObjectId>, KuduInstance<ObjectId>>
                {
                    Projection = Builders<KuduInstance<ObjectId>>.Projection.Include(x => x.Id)
                },
                cancellation);

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance == null ? ObjectId.Empty : kuduInstance.Id;
        }

        protected override async Task<string> GetNameByIdImplementation(ObjectId id, CancellationToken cancellation)
        {
            if (id == ObjectId.Empty)
                return default;

            var collection = await _collectionFactory.Get<KuduInstance<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Id == id,
                new FindOptions<KuduInstance<ObjectId>, KuduInstance<ObjectId>>
                {
                    Projection = Builders<KuduInstance<ObjectId>>.Projection.Include(x => x.Name)
                },
                cancellation); ;

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance?.Name;
        }
    }
}