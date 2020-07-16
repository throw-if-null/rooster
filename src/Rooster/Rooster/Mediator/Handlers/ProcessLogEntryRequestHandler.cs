using MediatR;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class ProcessLogEntryRequestHandler<T> : AsyncRequestHandler<ProcessLogEntryRequest<T>>
    {
        private readonly ILogEntryRepository<T> _logEntryRepository;

        protected ProcessLogEntryRequestHandler(ILogEntryRepository<T> logEntryRepository)
        {
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
        }

        protected override async Task Handle(ProcessLogEntryRequest<T> request, CancellationToken cancellationToken)
        {
            var latestLogEntry =
                await
                    _logEntryRepository.GetLatestByServiceAndContainerNames(
                        request.ExportedLogEntry.ServiceName,
                        request.ExportedLogEntry.ContainerName,
                        cancellationToken);

            if (request.ExportedLogEntry.EventDate <= latestLogEntry)
                return;

            var logEntry = new LogEntry<T>
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

            await _logEntryRepository.Create(logEntry, cancellationToken);
        }
    }
}
