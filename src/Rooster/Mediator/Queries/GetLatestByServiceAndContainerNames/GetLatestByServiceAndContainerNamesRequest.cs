using MediatR;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames
{
    public sealed record GetLatestByServiceAndContainerNamesRequest  :
        IRequest<DateTimeOffset>,
        IRequestProcessingErrorBehavior
    {
        public string ServiceName { get; init; }

        public string ContainerName { get; init; }

        public void OnError([NotNull] Exception ex)
        {
            return;
        }
    }
}