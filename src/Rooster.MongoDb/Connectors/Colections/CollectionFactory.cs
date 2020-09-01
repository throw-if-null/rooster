using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rooster.MongoDb.Connectors.Databases;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Connectors.Colections
{
    public interface ICollectionFactory
    {
        Task<IMongoCollection<T>> Get<T>(CancellationToken cancellationToken);
    }

    public abstract class CollectionFactory<TOptions> : ICollectionFactory where TOptions : CollectionFactoryOptions, new()
    {
        private readonly IDatabaseFactory _databaseFactory;
        private readonly TOptions _options;

        public CollectionFactory(IOptions<TOptions> options, IDatabaseFactory databaseFactory)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _databaseFactory = databaseFactory ?? throw new ArgumentNullException(nameof(databaseFactory));
        }

        public async Task<IMongoCollection<T>> Get<T>(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.Name))
                throw new ArgumentNullException("CollectionName not configured.");

            var database = await _databaseFactory.Get(cancellationToken).ConfigureAwait(false);
            IMongoCollection<T> collection = database.GetCollection<T>(_options.Name);

            return collection;
        }
    }
}