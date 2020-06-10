using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices.Implementations.MongoDb
{
    public class MongoDbAppServiceRepository : IMongoDbAppServiceRepository
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

        public async Task<MongoDbAppService> Create(MongoDbAppService appService, CancellationToken cancellation)
        {
            _ = appService ?? throw new ArgumentNullException(nameof(appService));

            var collection = await _collectionFactory.Get<MongoDbAppService>(cancellation);

            await collection.InsertOneAsync(appService, GetInsertOneOptions(), cancellation);

            return appService;
        }

        public async Task<MongoDbAppService> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            var collection = await _collectionFactory.Get<MongoDbAppService>(cancellation);

            var cursor = await collection.FindAsync(x => x.Name == name.Trim(), null, cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService;
        }

        public async Task<string> GetNameById(string id, CancellationToken cancellation)
        {
            if (ObjectId.TryParse(id, out var objectId))
                return default;

            var collection = await _collectionFactory.Get<MongoDbAppService>(cancellation);

            var cursor = await collection.FindAsync(x => x.Id == objectId, null, cancellation);

            var appService = await cursor.FirstOrDefaultAsync();

            return appService.Name;
        }
    }
}