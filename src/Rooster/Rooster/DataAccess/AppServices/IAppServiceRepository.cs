using Rooster.DataAccess.AppServices.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices
{
    public interface IAppServiceRepository<T>
    {
        bool IsDefaultValue(T value);

        Task<T> Create(AppService<T> appService, CancellationToken cancellation);

        Task<T> GetIdByName(string name, CancellationToken cancellation);

        Task<string> GetNameById(T id, CancellationToken cancellation);
    }
}