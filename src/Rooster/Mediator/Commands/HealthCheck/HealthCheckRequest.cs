using MediatR;

namespace Rooster.Mediator.Commands.HealthCheck
{
    public abstract record HealthCheckRequest : IRequest<HealthCheckResponse>
    {
    }
}
