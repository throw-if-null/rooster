using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.DataAccess.ContainerInstances;
using Rooster.DataAccess.ContainerInstances.Entities;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.DataAccess.ContainerInstances
{
    public class MongoDbContainerInstanceRepository : ContainerInstanceRepository<ObjectId>
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly IKuduInstanceCollectionFactory _collectionFactory;

        public MongoDbContainerInstanceRepository(IKuduInstanceCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<ObjectId> CreateImplementation(ContainerInstance<ObjectId> kuduInstance, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<ContainerInstance<ObjectId>>(cancellation);

            await collection.InsertOneAsync(kuduInstance, GetInsertOneOptions(), cancellation);

            return kuduInstance.Id;
        }

        protected override async Task<ObjectId> GetIdByNameAndAppServiceIdImplementation(string name, ObjectId appServiceId, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<ContainerInstance<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Name == name && x.AppServiceId == appServiceId,
                new FindOptions<ContainerInstance<ObjectId>, ContainerInstance<ObjectId>>
                {
                    Projection = Builders<ContainerInstance<ObjectId>>.Projection.Include(x => x.Id)
                },
                cancellation);

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance == null ? ObjectId.Empty : kuduInstance.Id;
        }

        protected override async Task<string> GetNameByIdImplementation(ObjectId id, CancellationToken cancellation)
        {
            if (id == ObjectId.Empty)
                return default;

            var collection = await _collectionFactory.Get<ContainerInstance<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Id == id,
                new FindOptions<ContainerInstance<ObjectId>, ContainerInstance<ObjectId>>
                {
                    Projection = Builders<ContainerInstance<ObjectId>>.Projection.Include(x => x.Name)
                },
                cancellation); ;

            var kuduInstance = await cursor.FirstOrDefaultAsync();

            return kuduInstance?.Name;
        }

        public override bool IsDefaultValue(ObjectId value)
        {
            return value == ObjectId.Empty;
        }
    }
}