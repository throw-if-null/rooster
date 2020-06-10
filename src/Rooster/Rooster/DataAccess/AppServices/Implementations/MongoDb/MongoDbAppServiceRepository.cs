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

            var collection = await _collectionFactory.Get<IAppService>(cancellation);

            await collection.InsertOneAsync(appService, GetInsertOneOptions(), cancellation);

            return appService;
        }

        public Task<MongoDbAppService> GetIdByName(string name, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNameById(int id, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}