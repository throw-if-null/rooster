using MediatR;
using Rooster.Mediator.Commands.ExtractDockerRunParams;

namespace Rooster.Mediator.Commands.ShouldProcessDockerLog
{
    public record ShouldProcessDockerLogRequest : IRequest
    {
        public ExtractDockerRunParamsResponse ExportedLogEntry { get; set; }
    }
}