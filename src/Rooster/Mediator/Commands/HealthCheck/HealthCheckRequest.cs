using MediatR;

namespace Rooster.Mediator.Commands.HealthCheck
{
    public abstract class HealthCheckRequest : IRequest<HealthCheckResponse>
    {
    }
}
