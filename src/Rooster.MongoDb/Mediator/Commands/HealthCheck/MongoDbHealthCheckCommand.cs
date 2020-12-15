using MongoDB.Driver;
using Rooster.CrossCutting;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Schema;
using Rooster.QoS.Resilency;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.MongoDb.Mediator.Commands.HealthCheck
{
    public class MongoDbHealthCheckCommand : HealthCheckCommand
    {
        private readonly ILogEntryCollectionFactory _collectionFactory;
        private readonly IRetryProvider _retryProvider;

        public MongoDbHealthCheckCommand(ILogEntryCollectionFactory collectionFactory, IRetryProvider retryProvider)
        {
            _collectionFactory = collectionFactory;
            _retryProvider = retryProvider;
        }

        public override async Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
        {
            try
            {
                IMongoCollection<LogEntry> logEntries = await _collectionFactory.Get<LogEntry>(cancellationToken);

                var response =
                    await
                        _retryProvider.RetryOn<MongoConnectionException, long>(
                            x => true,
                            x => false,
                            () => logEntries.CountDocumentsAsync(new FilterDefinitionBuilder<LogEntry>().Empty));

                return Healthy(Engine.MongoDb.Name);
            }
            catch (Exception ex)
            {
                return Unhealthy(Engine.MongoDb.Name, ex.ToString());
            }
        }
    }
}