using MediatR;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.ExtractDockerRunParams
{
    public sealed record ExtractDockerRunParamsRequest :
        IRequest<ExtractDockerRunParamsResponse>,
        IRequestProcessingErrorBehavior
    {
        public string LogLine { get; init; }

        public void OnError([NotNull] Exception ex)
        {
            throw ex;
        }
    }
}