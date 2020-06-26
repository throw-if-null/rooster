using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.DataAccess.AppServices
{
    public class MongoDbAppServiceRepository : AppServiceRepository<ObjectId>
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly IAppServiceCollectionFactory _collectionFactory;

        public MongoDbAppServiceRepository(IAppServiceCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<ObjectId> CreateImplementation(AppService<ObjectId> appService, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            await collection.InsertOneAsync(appService, GetInsertOneOptions(), cancellation);

            return appService.Id;
        }

        protected override async Task<ObjectId> GetIdByNameImplementation(string name, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Name == name,
                new FindOptions<AppService<ObjectId>, AppService<ObjectId>>
                {
                    Projection = Builders<AppService<ObjectId>>.Projection.Include(x => x.Id)
                },
                cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService == null ? ObjectId.Empty : appService.Id;
        }

        protected override async Task<string> GetNameByIdImplementation(ObjectId id, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(
                x => x.Id == id,
                new FindOptions<AppService<ObjectId>, AppService<ObjectId>>
                {
                    Projection = Builders<AppService<ObjectId>>.Projection.Include(x => x.Name)
                },
                cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService?.Name;
        }

        public override bool IsDefaultValue(ObjectId value)
        {
            return value == ObjectId.Empty;
        }
    }
}