using MediatR;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessLogEntry
{
    public class ProcessLogEntryCommand : AsyncRequestHandler<ProcessLogEntryRequest>
    {
        private readonly IMediator _mediator;

        public ProcessLogEntryCommand(IMediator mediator)
        {
            _mediator = mediator;
        }

        protected override async Task Handle(ProcessLogEntryRequest request, CancellationToken cancellationToken)
        {
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

            CreateLogEntryRequest createLogEntry = request.ExportedLogEntry;

            await _mediator.Send(createLogEntry, cancellationToken);
        }
    }
}