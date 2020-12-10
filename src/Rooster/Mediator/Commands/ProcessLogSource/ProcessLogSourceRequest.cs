using MediatR;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.ProcessLogSource
{
    public sealed record ProcessLogSourceRequest :
        IRequest,
        IRequestProcessingErrorBehavior
    {
        public IKuduApiAdapter Kudu { get; init; }

        public DateTimeOffset LastUpdated { get; init; }

        public Uri LogUri { get; init; }

        public string MachineName { get; init; }

        public void OnError([NotNull] Exception ex)
        {
            return;
        }
    }
}