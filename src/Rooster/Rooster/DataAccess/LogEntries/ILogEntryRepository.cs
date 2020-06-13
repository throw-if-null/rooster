using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public interface ILogEntryRepository<T> where T : ILogEntry
    {
        Task Create(T entry, CancellationToken cancellation);
        Task<DateTimeOffset> GetLatestForAppService(string appServiceId, CancellationToken cancellation);
    }
}