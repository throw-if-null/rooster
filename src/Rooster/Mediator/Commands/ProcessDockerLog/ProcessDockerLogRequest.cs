using MediatR;
using Rooster.Mediator.Commands.Common.Behaviors;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.ProcessDockerLog
{
    public record ProcessDockerLogRequest :
        IRequest,
        IRequestProcessingErrorBehavior
    {
        public ExtractDockerRunParamsResponse ExportedLogEntry { get; set; }

        public void OnError([NotNull] Exception ex)
        {
            return;
        }
    }
}