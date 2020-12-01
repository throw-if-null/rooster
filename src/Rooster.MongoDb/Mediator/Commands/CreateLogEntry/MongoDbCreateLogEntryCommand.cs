using MediatR;
using MongoDB.Driver;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Commands.CreateLogEntry
{
    public sealed class MongoDbCreateLogEntryCommand : ValidateDockerRunParamsCommand
    {
        private static readonly Lazy<InsertOneOptions> GetInsertOneOptions = new Lazy<InsertOneOptions>(() => new InsertOneOptions(), true);

        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbCreateLogEntryCommand(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<Unit> CreateImplementation(ValidateDockerRunParamsRequest request, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<ValidateDockerRunParamsRequest>(cancellation);

            await collection.InsertOneAsync(request, GetInsertOneOptions.Value, cancellation);

            return Unit.Value;
        }
    }
}