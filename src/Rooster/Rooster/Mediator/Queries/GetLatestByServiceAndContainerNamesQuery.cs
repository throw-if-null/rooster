using MediatR;
using Rooster.Mediator.Queries.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Queries
{
    public abstract class GetLatestByServiceAndContainerNamesQuery
        : IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>
    {
        protected abstract Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(GetLatestByServiceAndContainerNamesRequest request, CancellationToken cancellation);

        public Task<DateTimeOffset> Handle(GetLatestByServiceAndContainerNamesRequest request, CancellationToken cancellationToken)
        {
            return GetLatestByServiceAndContainerNamesImplementation(request, cancellationToken);
        }
    }
}