using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public class NullLogEntryRepository : LogEntryRepository<object>
    {
        protected override bool IsDefaultValue(object value) => false;

        protected override Task CreateImplementation(LogEntry<object> logEntry, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        protected override Task<DateTimeOffset> GetLatestForLogbookImplementation(object logbookId, CancellationToken cancellation)
        {
            return Task.FromResult(DateTimeOffset.MinValue);
        }
    }
}