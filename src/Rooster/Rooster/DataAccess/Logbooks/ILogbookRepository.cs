using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public interface ILogbookRepository<T>
    {
        Task Create(Logbook<T> logbook, CancellationToken cancellation);

        Task<DateTimeOffset> GetLastUpdateDateForKuduInstance(T kuduInstanceId, CancellationToken cancellation);
    }
}