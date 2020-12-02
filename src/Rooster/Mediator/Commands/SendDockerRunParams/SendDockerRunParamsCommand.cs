using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.SendDockerRunParams
{
    public abstract class SendDockerRunParamsCommand : IRequestHandler<SendDockerRunParamsRequest>
    {
        protected abstract Task<Unit> CreateImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation);

        public Task<Unit> Handle(SendDockerRunParamsRequest request, CancellationToken cancellationToken)
        {
            return CreateImplementation(request, cancellationToken);
        }
    }
}
