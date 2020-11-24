using MediatR;

namespace Rooster.Mediator.Commands.ExtractDockerRunParams
{
    public class ExtractDockerRunParamsRequest : IRequest<ExtractDockerRunParamsResponse>
    {
        public string LogLine { get; set; }
    }
}