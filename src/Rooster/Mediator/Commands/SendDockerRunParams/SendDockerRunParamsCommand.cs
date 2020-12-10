using MediatR;
using Rooster.Mediator.Commands.Common;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.SendDockerRunParams
{
    public abstract class SendDockerRunParamsCommand : IOpinionatedRequestHandler<SendDockerRunParamsRequest, Unit>
    {
        protected abstract Task<Unit> SendImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation);

        public Task<Unit> Handle(SendDockerRunParamsRequest request, CancellationToken cancellationToken)
        {
            return SendImplementation(request, cancellationToken);
        }
    }
}
