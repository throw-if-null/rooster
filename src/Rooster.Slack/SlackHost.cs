using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using System.Collections.Generic;

namespace Rooster.Slack
{
    public class SlackHost : AppHost
    {
        public SlackHost(
            IOptionsMonitor<AppHostOptions> options,
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<SlackHost> logger)
            : base(options, lifetime, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SlackHost)} started.";

        protected override string StopLogMessage => $"{nameof(SlackHost)} stopped.";
    }
}