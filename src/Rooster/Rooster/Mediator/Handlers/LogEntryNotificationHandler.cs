using MediatR;
using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.Mediator.Notifications;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class LogEntryNotificationHandler<T> : INotificationHandler<LogEntryNotification<T>>
    {
        public ILogEntryRepository<T> LogEntryRepository { get; }

        public ILogExtractor Extractor { get; }

        public LogEntryNotificationHandler(ILogEntryRepository<T> logEntryRepository, ILogExtractor extractor)
        {
            LogEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            Extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        public virtual async Task Handle(LogEntryNotification<T> notification, CancellationToken cancellationToken)
        {
            var (inboundPort, outboundPort) = Extractor.ExtractPorts(notification.LogLine);

            var logEntry = new LogEntry<T>(
                notification.ContainerInstanceId,
                Extractor.ExtractImageName(notification.LogLine),
                Extractor.ExtractContainerName(notification.LogLine),
                inboundPort,
                outboundPort,
                Extractor.ExtractDate(notification.LogLine));

            var latestLogEntry = await LogEntryRepository.GetLatestForLogbook(logEntry.LogbookId, cancellationToken);

            if (logEntry.Date <= latestLogEntry)
                return;

            await LogEntryRepository.Create(logEntry, cancellationToken);
        }
    }
}