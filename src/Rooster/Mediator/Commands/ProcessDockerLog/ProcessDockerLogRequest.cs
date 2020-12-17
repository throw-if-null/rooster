using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.ProcessDockerLog
{
    public record ProcessDockerLogRequest :
        IRequest,
        IRequestProcessingErrorBehavior
    {
        public DockerRunParams ExtractedParams { get; set; }

        public void OnError([NotNull] Exception ex)
        {
            return;
        }
    }
}