using MediatR;
using Rooster.Mediator.Commands.Common.Behaviors;

namespace Rooster.Mediator.Commands.Common
{
    public interface IOpinionatedRequestHandler<TRequest, TResponse> :
        IRequestHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>, IRequestProcessingErrorBehavior
    {
    }
}
