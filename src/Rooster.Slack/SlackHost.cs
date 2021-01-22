using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Hosting;

namespace Rooster.Slack
{
    public class SlackHost : PollerHost
    {
        public SlackHost(
            IMediator mediator,
            ILogger<SlackHost> logger)
            : base(mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SlackHost)} started.";

        protected override string StopLogMessage => $"{nameof(SlackHost)} stopped.";
    }
}