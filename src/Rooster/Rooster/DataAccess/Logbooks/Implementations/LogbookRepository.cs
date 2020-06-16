using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations
{
    public abstract class LogbookRepository<T> : ILogbookRepository<T>
    {
        protected abstract bool IsDefaultValue(T value);
        protected abstract Task CreateImplementation(Logbook<T> logbook, CancellationToken cancellation);

        public Task Create(Logbook<T> logbook, CancellationToken cancellation)
        {
            ValidateLogbook(logbook);

            return CreateImplementation(logbook, cancellation);
        }

        public abstract Task<Logbook<T>> GetLast(CancellationToken cancellation);

        private void ValidateLogbook(Logbook<T> logbook)
        {
            _ = logbook ?? throw new ArgumentNullException(nameof(logbook));

            if (IsDefaultValue(logbook.KuduInstanceId))
                ThrowArgumentException(nameof(logbook.KuduInstanceId), logbook.KuduInstanceId.ToString());

            if (logbook.LastUpdated == default || logbook.LastUpdated == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logbook.LastUpdated), logbook.LastUpdated.ToString());

            if (string.IsNullOrWhiteSpace(logbook.MachineName))
                ThrowArgumentException(nameof(logbook.MachineName), logbook.MachineName == null ? "NULL" : "EMPTY");

            logbook.MachineName = logbook.MachineName.Trim().ToLowerInvariant();
        }

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };
    }
}