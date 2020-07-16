using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public class NopLogEntryRepository : LogEntryRepository<Nop>
    {
        protected override bool IsDefaultValue(Nop value) => false;

        protected override Task CreateImplementation(LogEntry<Nop> logEntry, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        protected override Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(string serviceName, string containerName, CancellationToken cancellation)
        {
            return Task.FromResult(DateTimeOffset.MinValue);
        }
    }
}