using MediatR;
using System;

namespace Rooster.Mediator.Queries.Requests
{
    public class GetLatestByServiceAndContainerNamesRequest  : IRequest<DateTimeOffset>
    {
        public string ServiceName { get; set; }

        public string ContainerName { get; set; }
    }
}