using Rooster.DataAccess.AppServices.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices
{
    public interface IAppServiceRepository<T> where T : IAppService
    {
        Task<T> Create(T appService, CancellationToken cancellation);

        Task<T> GetIdByName(string name, CancellationToken cancellation);

        Task<string> GetNameById(string id, CancellationToken cancellation);
    }
}