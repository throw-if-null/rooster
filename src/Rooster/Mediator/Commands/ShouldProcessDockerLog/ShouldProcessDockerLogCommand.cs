using MediatR;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
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
            ValidateExportedRunParamsRequest validateExportedRunParamsRequest = request.ExportedLogEntry;
            ValidateExportedRunParamsResponse validateExportedRunParamsResponse =
                await _mediator.Send(validateExportedRunParamsRequest);

            if (!validateExportedRunParamsResponse.IsValid)
                return;

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

            SendDockerRunParamsRequest sendDockerRunParamsRequest = request.ExportedLogEntry;
            await _mediator.Send(sendDockerRunParamsRequest, cancellationToken);
        }
    }
}