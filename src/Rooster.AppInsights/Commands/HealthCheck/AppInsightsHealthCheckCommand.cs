using Rooster.CrossCutting;
using Rooster.Mediator.Commands.HealthCheck;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppInsights.Handlers.HealthCheck
{
    /// <summary>
    /// Right now I don't know a way to check if sending event to AI is possible, so this implementation always return `healthy`.
    /// </summary>
    public class AppInsightsHealthCheckCommand : HealthCheckCommand
    {
        public override Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Healthy(Engines.AppInsights));
        }
    }
}
