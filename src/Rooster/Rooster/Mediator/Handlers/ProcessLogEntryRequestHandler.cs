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
                        request.ServiceName,
                        request.ContainerName,
                        cancellationToken);

            if (request.EventDate <= latestLogEntry)
                return;

            var logEntry = new LogEntry<T>
            {
                Id = request.Id,
                Created = request.Created,
                ServiceName = request.ServiceName,
                ContainerName = request.ContainerName,
                ImageName = request.ImageName,
                ImageTag = request.ImageTag,
                InboundPort = request.InboundPort,
                OutboundPort = request.OutboundPort,
                EventDate = request.EventDate
            };

            await _logEntryRepository.Create(logEntry, cancellationToken);
        }
    }
}
