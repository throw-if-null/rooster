using MediatR;

namespace Rooster.Mediator.Commands.ExtractDockerRunParams
{
    public record ExtractDockerRunParamsRequest : IRequest<ExtractDockerRunParamsResponse>
    {
        public string LogLine { get; init; }
    }
}