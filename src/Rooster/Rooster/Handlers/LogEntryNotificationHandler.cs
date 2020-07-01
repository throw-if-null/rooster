using MediatR;
using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Handlers
{
    public class LogEntryNotification<T> : INotification
    {
        public string LogLine { get; set; }

        public T ContainerInstanceId { get; set; }
    }

    public abstract class LogEntryNotificationHandler<T> : INotificationHandler<LogEntryNotification<T>>
    {
        private readonly ILogEntryRepository<T> _logEntryRepository;
        private readonly ILogExtractor _extractor;

        public LogEntryNotificationHandler(ILogEntryRepository<T> logEntryRepository, ILogExtractor extractor, IMediator mediator)
        {
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
        }

        public async Task Handle(LogEntryNotification<T> notification, CancellationToken cancellationToken)
        {
            var (inboundPort, outboundPort) = _extractor.ExtractPorts(notification.LogLine);

            var logEntry = new LogEntry<T>(
                notification.ContainerInstanceId,
                _extractor.ExtractImageName(notification.LogLine),
                _extractor.ExtractContainerName(notification.LogLine),
                inboundPort,
                outboundPort,
                _extractor.ExtractDate(notification.LogLine));

            var latestLogEntry = await _logEntryRepository.GetLatestForLogbook(logEntry.LogbookId, cancellationToken);

            if (logEntry.Date <= latestLogEntry)
                return;

            await _logEntryRepository.Create(logEntry, cancellationToken);
        }
    }
}
