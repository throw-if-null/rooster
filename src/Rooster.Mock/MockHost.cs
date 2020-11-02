using MediatR;
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
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<MockHost> logger)
            : base(options, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(MockHost)} started.";
        protected override string StopLogMessage => $"{nameof(MockHost)} stopped.";
    }
}
