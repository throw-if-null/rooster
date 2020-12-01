using MediatR;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ShouldProcessDockerLog
{
    public class ShouldProcessDockerLogCommand : AsyncRequestHandler<ShouldProcessDockerLogRequest>
    {
        private readonly IMediator _mediator;

        public ShouldProcessDockerLogCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override async Task Handle(ShouldProcessDockerLogRequest request, CancellationToken cancellationToken)
        {
            ValidateDockerRunParamsRequest validateDockerRunParamsRequest = request.ExportedLogEntry;
            await _mediator.Send(validateDockerRunParamsRequest, cancellationToken);

            var latestLogEntry =
                await
                    _mediator.Send(
                        new GetLatestByServiceAndContainerNamesRequest
                        {
                            ContainerName = request.ExportedLogEntry.ContainerName,
                            ServiceName = request.ExportedLogEntry.ServiceName
                        },
                        cancellationToken);

            if (request.ExportedLogEntry.EventDate <= latestLogEntry)
                return;
        }
    }
}