using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.Connectors.MongoDb.Colections;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries.Implementations
{
    public class MongoDbLogEntryRepository : ILogEntryRepository<ObjectId>
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbLogEntryRepository(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        public async Task Create(LogEntry<ObjectId> entry, CancellationToken cancellation)
        {
            _ = entry ?? throw new ArgumentNullException(nameof(entry));

            var collection = await _collectionFactory.Get<LogEntry<ObjectId>>(cancellation);

            entry.ContainerName = entry.ContainerName.Trim().ToLowerInvariant();
            entry.HostName = entry.HostName.Trim().ToLowerInvariant();
            entry.ImageName = entry.ImageName.Trim().ToLowerInvariant();

            await collection.InsertOneAsync(entry, GetInsertOneOptions(), cancellation);
        }


        public async Task<DateTimeOffset> GetLatestForAppService(ObjectId appServiceId, CancellationToken cancellation)
        {
            if (appServiceId == ObjectId.Empty)
                return default;

            var collection = await _collectionFactory.Get<LogEntry<ObjectId>>(cancellation);

            var filter = Builders<LogEntry<ObjectId>>.Filter.Where(x => x.AppServiceId == appServiceId);
            var sort = Builders<LogEntry<ObjectId>>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<LogEntry<ObjectId>, LogEntry<ObjectId>>
            {
                Sort = sort,
                Limit = 1,
                Projection = Builders<LogEntry<ObjectId>>.Projection.Include(x => x.Date)
            });

            var entry = await cursor.FirstOrDefaultAsync();

            return entry == null ? default : entry.Date;
        }

        private static void Validate(LogEntry<ObjectId> logEntry)
        {
            _ = logEntry ?? throw new ArgumentNullException(nameof(logEntry));

            if (logEntry.AppServiceId == ObjectId.Empty)
                ThrowArgumentException(nameof(logEntry.AppServiceId), logEntry.AppServiceId.ToString());

            if (string.IsNullOrWhiteSpace(logEntry.ContainerName))
                ThrowArgumentException(nameof(logEntry.ContainerName), logEntry.ContainerName == null ? "NULL" : "EMPTY");

            if (logEntry.Date == default || logEntry.Date == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logEntry.Date), logEntry.Date.ToString());

            if (string.IsNullOrWhiteSpace(logEntry.HostName))
                ThrowArgumentException(nameof(logEntry.HostName), logEntry.HostName == null ? "NULL" : "EMPTY");

            if (string.IsNullOrWhiteSpace(logEntry.ImageName))
                ThrowArgumentException(nameof(logEntry.ImageName), logEntry.ImageName == null ? "NULL" : "EMPTY");

            if (logEntry.InboundPort == default)
                ThrowArgumentException(nameof(logEntry.InboundPort), logEntry.InboundPort);

            if (logEntry.OutboundPort == default)
                ThrowArgumentException(nameof(logEntry.OutboundPort), logEntry.InboundPort);
        }

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };
    }
}