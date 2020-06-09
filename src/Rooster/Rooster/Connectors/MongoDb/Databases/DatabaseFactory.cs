using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Clients;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Connectors.MongoDb.Databases
{
    public interface IDatabaseFactory
    {
        Task<IMongoDatabase> Get(CancellationToken cancellationToken);
    }

    public sealed class DatabaseFactory : IDatabaseFactory
    {
        private readonly ConcurrentDictionary<string, IMongoDatabase> _cache = new ConcurrentDictionary<string, IMongoDatabase>();

        private readonly DatabaseFactoryOptions _options;
        private readonly IMongoClient _client;
        private readonly ILogger _logger;

        public DatabaseFactory(IOptions<DatabaseFactoryOptions> options, IClientFactory clientFactory, ILogger<DatabaseFactory> logger)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _ = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _client = clientFactory.Create();
        }

        public async Task<IMongoDatabase> Get(CancellationToken cancellationToken)
        {
            if (_cache.ContainsKey(_options.Name))
                return _cache[_options.Name];

            var names = await (await _client.ListDatabaseNamesAsync(cancellationToken).ConfigureAwait(false)).ToListAsync(cancellationToken).ConfigureAwait(false);

            if (!names.Any(x => x.Equals(_options.Name.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                throw new ArgumentOutOfRangeException($"Database: {_options.Name} doesn't exist.");

            _cache[_options.Name] = _client.GetDatabase(_options.Name);
            _logger.LogInformation($"Database initialized {_options.Name}.");

            return _cache[_options.Name];
        }
    }
}