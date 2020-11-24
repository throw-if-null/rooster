using MediatR;
using Rooster.Mediator.Commands.ExtractDockerRunParams;

namespace Rooster.Mediator.Commands.ProcessLogEntry
{
    public class ShouldProcessDockerLogRequest : IRequest
    {
        public ExtractDockerRunParamsResponse ExportedLogEntry { get; set; }
    }
}