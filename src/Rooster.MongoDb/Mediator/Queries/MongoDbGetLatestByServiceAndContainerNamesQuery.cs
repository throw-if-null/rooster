using MongoDB.Driver;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Queries
{
    public sealed class MongoDbGetLatestByServiceAndContainerNamesQuery : GetLatestByServiceAndContainerNamesQuery
    {
        private static readonly Lazy<FindOptions<LogEntry, LogEntry>> GetFindOptions = new Lazy<FindOptions<LogEntry, LogEntry>>(
            () =>
                new FindOptions<LogEntry, LogEntry>
                {
                    Limit = 1,
                    Sort = Builders<LogEntry>.Sort.Descending(x => x.Created),
                    Projection = Builders<LogEntry>.Projection.Include(x => x.EventDate)
                });


        private readonly ILogEntryCollectionFactory _collectionFactory;

    public MongoDbGetLatestByServiceAndContainerNamesQuery(ILogEntryCollectionFactory collectionFactory)
    {
        _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
    }

    protected override async Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(
        GetLatestByServiceAndContainerNamesRequest request,
        CancellationToken cancellation)
    {
        var collection = await _collectionFactory.Get<LogEntry>(cancellation);

        var filter =
            Builders<LogEntry>.Filter.Where(x =>
                x.ServiceName == request.ServiceName &&
                x.ContainerName == request.ContainerName);

        var cursor = await collection.FindAsync(filter, GetFindOptions.Value, cancellation);

        var entry = await cursor.FirstOrDefaultAsync(cancellation);

        return entry == null ? default : entry.EventDate;
    }
}
}