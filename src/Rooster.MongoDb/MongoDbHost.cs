using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using System.Collections.Generic;

namespace Rooster.MongoDb
{
    public class MongoDbHost : AppHost
    {
        public MongoDbHost(
            IOptionsMonitor<AppHostOptions> options,
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<MongoDbHost> logger)
            : base(options, lifetime, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(MongoDbHost)} started.";

        protected override string StopLogMessage => $"{nameof(MongoDbHost)} stopped.";
    }
}
