using MediatR;
using MongoDB.Driver;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Commands.CreateLogEntry
{
    public sealed class MongoDbCreateLogEntryCommand : CreateLogEntryCommand
    {
        private static readonly Func<InsertOneOptions> GetInsertOneOptions = delegate
        {
            return new InsertOneOptions();
        };

        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbCreateLogEntryCommand(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<Unit> CreateImplementation(CreateLogEntryRequest request, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<CreateLogEntryRequest>(cancellation);

            await collection.InsertOneAsync(request, GetInsertOneOptions(), cancellation);

            return Unit.Value;
        }
    }
}
