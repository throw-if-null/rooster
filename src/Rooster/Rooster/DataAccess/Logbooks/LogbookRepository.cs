using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public abstract class LogbookRepository<T> : ILogbookRepository<T>
    {
        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };

        protected abstract bool IsDefaultValue(T value);

        protected abstract Task CreateImplementation(Logbook<T> logbook, CancellationToken cancellation);

        protected abstract Task<DateTimeOffset> GetLastUpdatedDateForContainerInstanceImplementation(
            T kuduInstanceId,
            CancellationToken cancellation);

        public Task Create(Logbook<T> logbook, CancellationToken cancellation)
        {
            ValidateLogbook(logbook);

            return CreateImplementation(logbook, cancellation);
        }

        private void ValidateLogbook(Logbook<T> logbook)
        {
            _ = logbook ?? throw new ArgumentNullException(nameof(logbook));

            if (IsDefaultValue(logbook.ContainerInstanceId))
                ThrowArgumentException(nameof(logbook.ContainerInstanceId), logbook.ContainerInstanceId.ToString());

            if (logbook.LastUpdated == default || logbook.LastUpdated == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logbook.LastUpdated), logbook.LastUpdated.ToString());

            if (string.IsNullOrWhiteSpace(logbook.MachineName))
                ThrowArgumentException(nameof(logbook.MachineName), logbook.MachineName == null ? "NULL" : "EMPTY");

            logbook.MachineName = logbook.MachineName.Trim().ToLowerInvariant();
        }

        public Task<DateTimeOffset> GetLastUpdatedDateForContainerInstance(T kuduInstanceId, CancellationToken cancellation)
        {
            if (IsDefaultValue(kuduInstanceId))
                return default;

            return GetLastUpdatedDateForContainerInstanceImplementation(kuduInstanceId, cancellation);
        }
    }
}