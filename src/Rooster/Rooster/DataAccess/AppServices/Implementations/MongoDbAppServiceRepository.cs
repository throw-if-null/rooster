using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices.Implementations
{
    public class MongoDbAppServiceRepository : IAppServiceRepository<ObjectId>
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

        public async Task<ObjectId> Create(AppService<ObjectId> appService, CancellationToken cancellation)
        {
            _ = appService ?? throw new ArgumentNullException(nameof(appService));

            if (string.IsNullOrWhiteSpace(appService.Name))
                throw new ArgumentException($"{nameof(appService.Name)} is required.");

            appService.Name = appService.Name.Trim().ToLowerInvariant();

            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            await collection.InsertOneAsync(appService, GetInsertOneOptions(), cancellation);

            return appService.Id;
        }

        public async Task<ObjectId> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            var trimmedName = name.Trim();
            var cursor = await collection.FindAsync(
                x => x.Name == trimmedName,
                new FindOptions<AppService<ObjectId>, AppService<ObjectId>>
                {
                    Projection = Builders<AppService<ObjectId>>.Projection.Include(x => x.Id)
                },
                cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService == null ? ObjectId.Empty : appService.Id;
        }

        public async Task<string> GetNameById(ObjectId id, CancellationToken cancellation)
        {
            if (id == ObjectId.Empty)
                return default;

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
    }
}