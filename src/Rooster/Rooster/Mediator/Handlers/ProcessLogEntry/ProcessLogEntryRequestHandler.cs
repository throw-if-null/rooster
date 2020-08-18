using MediatR;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers.ProcessLogEntry
{
    public class ProcessLogEntryRequestHandler : AsyncRequestHandler<ProcessLogEntryRequest>
    {
        private readonly IMediator _mediator;

        public ProcessLogEntryRequestHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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

            var createLogEntryRequest = new CreateLogEntryRequest
            {
                Created = request.ExportedLogEntry.Created,
                ServiceName = request.ExportedLogEntry.ServiceName,
                ContainerName = request.ExportedLogEntry.ContainerName,
                ImageName = request.ExportedLogEntry.ImageName,
                ImageTag = request.ExportedLogEntry.ImageTag,
                InboundPort = request.ExportedLogEntry.InboundPort,
                OutboundPort = request.ExportedLogEntry.OutboundPort,
                EventDate = request.ExportedLogEntry.EventDate
            };

            await _mediator.Send(createLogEntryRequest, cancellationToken);
        }
    }
}