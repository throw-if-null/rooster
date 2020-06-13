using Rooster.DataAccess.Logbooks.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public interface ILogbookRepository<T> where T : ILogbook
    {
        Task Create(T logbook, CancellationToken cancellation);

        Task<T> GetLast(CancellationToken cancellation);
    }
}