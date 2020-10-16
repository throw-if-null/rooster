using MediatR;
using Microsoft.Extensions.Hosting;
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
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<AppInsightsHost> logger)
            : base(options, lifetime, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(AppInsightsHost)} started.";

        protected override string StopLogMessage => $"{nameof(AppInsightsHost)} stopped.";
    }
}
