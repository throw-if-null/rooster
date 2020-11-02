using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using System.Collections.Generic;

namespace Rooster.AppInsights
{
    public class AppInsightsHost : AppHost
    {
        public AppInsightsHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<AppInsightsHost> logger)
            : base(options, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(AppInsightsHost)} started.";

        protected override string StopLogMessage => $"{nameof(AppInsightsHost)} stopped.";
    }
}
