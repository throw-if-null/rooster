using MediatR;
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
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<SlackHost> logger)
            : base(options, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SlackHost)} started.";

        protected override string StopLogMessage => $"{nameof(SlackHost)} stopped.";
    }
}