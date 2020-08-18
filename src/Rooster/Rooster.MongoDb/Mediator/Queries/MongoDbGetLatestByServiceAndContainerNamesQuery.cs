using MongoDB.Bson;
using MongoDB.Driver;
using Rooster.DataAccess.Entities;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Queries
{
    public sealed class MongoDbGetLatestByServiceAndContainerNamesQuery : GetLatestByServiceAndContainerNamesQuery
    {
        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbGetLatestByServiceAndContainerNamesQuery(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(
            GetLatestByServiceAndContainerNamesRequest request,
            CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<LogEntry<ObjectId>>(cancellation);

            var filter = Builders<LogEntry<ObjectId>>.Filter.Where(x => x.ServiceName == request.ServiceName && x.ContainerName == request.ContainerName);
            var sort = Builders<LogEntry<ObjectId>>.Sort.Descending(x => x.Created);

            var cursor = await collection.FindAsync(filter, new FindOptions<LogEntry<ObjectId>, LogEntry<ObjectId>>
            {
                Sort = sort,
                Limit = 1,
                Projection = Builders<LogEntry<ObjectId>>.Projection.Include(x => x.EventDate)
            });

            var entry = await cursor.FirstOrDefaultAsync();

            return entry == null ? default : entry.EventDate;
        }
    }
}