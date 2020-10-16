using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using System.Collections.Generic;

namespace Rooster.Mock
{
    public class MockHost : AppHost
    {
        public MockHost(
            IOptionsMonitor<AppHostOptions> options,
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<MockHost> logger)
            : base(options, lifetime, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(MockHost)} started.";
        protected override string StopLogMessage => $"{nameof(MockHost)} stopped.";
    }
}
