using Rooster.Mediator.Commands.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.HealthCheck
{
    public abstract class HealthCheckCommand :
        IOpinionatedRequestHandler<HealthCheckRequest, HealthCheckResponse>
    {
        public abstract Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken);

        public HealthCheckResponse Healthy(string name)
        {
            return new HealthCheckResponse { IsHaelthy = true, Message = "OK", Name = name };
        }

        public HealthCheckResponse Unhealthy(string name, string message)
        {
            return new HealthCheckResponse { IsHaelthy = false, Message = message, Name = name };
        }
    }
}
