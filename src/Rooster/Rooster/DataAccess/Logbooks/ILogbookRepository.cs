using Rooster.DataAccess.Logbooks.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public interface ILogbookRepository<T>
    {
        Task Create(Logbook<T> logbook, CancellationToken cancellation);

        Task<Logbook<T>> GetLast(CancellationToken cancellation);
    }
}