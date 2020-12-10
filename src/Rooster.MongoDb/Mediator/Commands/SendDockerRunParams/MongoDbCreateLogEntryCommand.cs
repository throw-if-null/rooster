using MediatR;
using MongoDB.Driver;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.MongoDb.Connectors.Colections;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Commands.SendDockerRunParams
{
    public sealed class MongoDbSendDockerRunParamsCommand : SendDockerRunParamsCommand
    {
        private static readonly Lazy<InsertOneOptions> GetInsertOneOptions = new Lazy<InsertOneOptions>(() => new InsertOneOptions(), true);

        private readonly ILogEntryCollectionFactory _collectionFactory;

        public MongoDbSendDockerRunParamsCommand(ILogEntryCollectionFactory collectionFactory)
        {
            _collectionFactory = collectionFactory ?? throw new ArgumentNullException(nameof(collectionFactory));
        }

        protected override async Task<Unit> SendImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation)
        {
            var collection = await _collectionFactory.Get<SendDockerRunParamsRequest>(cancellation);

            await collection.InsertOneAsync(request, GetInsertOneOptions.Value, cancellation);

            return Unit.Value;
        }
    }
}