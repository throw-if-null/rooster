using Rooster.Mediator.Commands.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames
{
    public abstract class GetLatestByServiceAndContainerNamesQuery :
        IOpinionatedRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>
    {
        protected abstract Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(
            GetLatestByServiceAndContainerNamesRequest request,
            CancellationToken cancellation);

        public Task<DateTimeOffset> Handle(
            GetLatestByServiceAndContainerNamesRequest request,
            CancellationToken cancellationToken)
        {
            return GetLatestByServiceAndContainerNamesImplementation(request, cancellationToken);
        }
    }
}