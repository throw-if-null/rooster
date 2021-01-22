using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Hosting;

namespace Rooster.AppInsights
{
    public class AppInsightsHost : PollerHost
    {
        public AppInsightsHost(
            IMediator mediator,
            ILogger<AppInsightsHost> logger)
            : base(mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(AppInsightsHost)} started.";

        protected override string StopLogMessage => $"{nameof(AppInsightsHost)} stopped.";
    }
}
