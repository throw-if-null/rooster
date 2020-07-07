using MediatR;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class LogEntryRequestHandler<T> : AsyncRequestHandler<LogEntryRequest<T>>
    {
        private readonly ILogEntryRepository<T> _logEntryRepository;

        protected LogEntryRequestHandler(ILogEntryRepository<T> logEntryRepository)
        {
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
        }

        protected override async Task Handle(LogEntryRequest<T> request, CancellationToken cancellationToken)
        {
            var latestLogEntry = await _logEntryRepository.GetLatestForLogbook(request.LogbookId, cancellationToken);

            if (request.Date <= latestLogEntry)
                return;

            var logEntry = new LogEntry<T>
            {
                ContainerName = request.ContainerName,
                Created = request.Created,
                Date = request.Date,
                Id = request.Id,
                ImageName = request.ImageName,
                InboundPort = request.InboundPort,
                LogbookId = request.LogbookId,
                OutboundPort = request.OutboundPort,
                WebsiteName = request.WebsiteName
            };

            await _logEntryRepository.Create(logEntry, cancellationToken);
        }
    }
}
