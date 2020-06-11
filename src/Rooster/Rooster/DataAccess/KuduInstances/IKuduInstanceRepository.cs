using Rooster.DataAccess.KuduInstances.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances
{
    public interface IKuduInstaceRepository<T> where T : IKuduInstance
    {
        Task<T> Create(T kuduInstance, CancellationToken cancellation);

        Task<T> GetIdByName(string name, CancellationToken cancellation);

        Task<string> GetNameById(string id, CancellationToken cancellation);
    }
}