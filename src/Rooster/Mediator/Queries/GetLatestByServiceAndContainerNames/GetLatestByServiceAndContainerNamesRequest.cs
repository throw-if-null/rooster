using MediatR;
using System;

namespace Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames
{
    public record GetLatestByServiceAndContainerNamesRequest  : IRequest<DateTimeOffset>
    {
        public string ServiceName { get; init; }

        public string ContainerName { get; init; }
    }
}