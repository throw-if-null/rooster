﻿using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public abstract class LogEntryRepository<T> : ILogEntryRepository<T>
    {
        protected abstract bool IsDefaultValue(T value);
        protected abstract Task CreateImplementation(LogEntry<T> logEntry, CancellationToken cancellation);
        protected abstract Task<DateTimeOffset> GetLatestImplementation(CancellationToken cancellation);


        public Task Create(LogEntry<T> entry, CancellationToken cancellation)
        {
            Validate(entry);

            return CreateImplementation(entry, cancellation);
        }

        public Task<DateTimeOffset> GetLatest(CancellationToken cancellation)
        {
            return GetLatestImplementation(cancellation);
        }

        private void Validate(LogEntry<T> logEntry)
        {
            _ = logEntry ?? throw new ArgumentNullException(nameof(logEntry));

            if (string.IsNullOrWhiteSpace(logEntry.ContainerName))
                ThrowArgumentException(nameof(logEntry.ContainerName), logEntry.ContainerName == null ? "NULL" : "EMPTY");

            if (logEntry.Date == default || logEntry.Date == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logEntry.Date), logEntry.Date.ToString());

            if (string.IsNullOrWhiteSpace(logEntry.ImageName))
                ThrowArgumentException(nameof(logEntry.ImageName), logEntry.ImageName == null ? "NULL" : "EMPTY");

            if (logEntry.InboundPort == default)
                ThrowArgumentException(nameof(logEntry.InboundPort), logEntry.InboundPort);

            if (logEntry.OutboundPort == default)
                ThrowArgumentException(nameof(logEntry.OutboundPort), logEntry.InboundPort);

            logEntry.ContainerName = logEntry.ContainerName.Trim().ToLowerInvariant();
            logEntry.ImageName = logEntry.ImageName.Trim().ToLowerInvariant();
        }

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };
    }
}
