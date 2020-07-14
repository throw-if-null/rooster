using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public abstract class LogEntryRepository<T> : ILogEntryRepository<T>
    {
        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };

        protected abstract bool IsDefaultValue(T value);

        protected abstract Task CreateImplementation(LogEntry<T> logEntry, CancellationToken cancellation);

        protected abstract Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(string serviceName, string containerName, CancellationToken cancellation);

        public Task Create(LogEntry<T> entry, CancellationToken cancellation)
        {
            Validate(entry);

            return CreateImplementation(entry, cancellation);
        }

        public Task<DateTimeOffset> GetLatestByServiceAndContainerNames(string serviceName, string containerName, CancellationToken cancellation)
        {
            return GetLatestByServiceAndContainerNamesImplementation(serviceName, containerName, cancellation);
        }

        private static void Validate(LogEntry<T> logEntry)
        {
            _ = logEntry ?? throw new ArgumentNullException(nameof(logEntry));

            if (string.IsNullOrWhiteSpace(logEntry.ServiceName))
                ThrowArgumentException(nameof(logEntry.ServiceName), logEntry.ServiceName == null ? "NULL" : "EMPTY");

            if (string.IsNullOrWhiteSpace(logEntry.ContainerName))
                ThrowArgumentException(nameof(logEntry.ContainerName), logEntry.ContainerName == null ? "NULL" : "EMPTY");

            if (string.IsNullOrWhiteSpace(logEntry.ImageName))
                ThrowArgumentException(nameof(logEntry.ImageName), logEntry.ImageName == null ? "NULL" : "EMPTY");

            if (string.IsNullOrWhiteSpace(logEntry.ImageTag))
                ThrowArgumentException(nameof(logEntry.ImageTag), logEntry.ImageTag == null ? "NULL" : "EMPTY");

            if (logEntry.InboundPort == default)
                ThrowArgumentException(nameof(logEntry.InboundPort), logEntry.InboundPort);

            if (logEntry.OutboundPort == default)
                ThrowArgumentException(nameof(logEntry.OutboundPort), logEntry.InboundPort);

            if (logEntry.EventDate == default || logEntry.EventDate == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logEntry.EventDate), logEntry.EventDate.ToString());

            logEntry.ServiceName = logEntry.ServiceName.Trim().ToLowerInvariant();
            logEntry.ContainerName = logEntry.ContainerName.Trim().ToLowerInvariant();
            logEntry.ImageName = logEntry.ImageName.Trim().ToLowerInvariant();
            logEntry.ImageTag = logEntry.ImageTag.Trim().ToLowerInvariant();
        }
    }
}