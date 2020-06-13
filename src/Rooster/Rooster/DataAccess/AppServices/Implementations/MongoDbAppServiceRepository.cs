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

            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            await collection.InsertOneAsync(appService, GetInsertOneOptions(), cancellation);

            return appService.Id;
        }

        public async Task<ObjectId> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(x => x.Name == name.Trim(), null, cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService.Id;
        }

        public async Task<string> GetNameById(ObjectId id, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<AppService<ObjectId>>(cancellation);

            var cursor = await collection.FindAsync(x => x.Id == id, null, cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService.Name;
        }
    }
}