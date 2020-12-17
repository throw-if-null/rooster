using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Hosting;

namespace Rooster.MongoDb
{
    public class MongoDbHost : AppHost
    {
        public MongoDbHost(
            IMediator mediator,
            ILogger<MongoDbHost> logger)
            : base(mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(MongoDbHost)} started.";

        protected override string StopLogMessage => $"{nameof(MongoDbHost)} stopped.";
    }
}
