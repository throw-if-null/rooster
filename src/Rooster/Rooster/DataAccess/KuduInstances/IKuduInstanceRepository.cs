using Rooster.DataAccess.KuduInstances.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances
{
    public interface IKuduInstanceRepository<T>
    {
        bool IsDefaultValue(T value);

        Task<T> Create(KuduInstance<T> kuduInstance, CancellationToken cancellation);

        Task<T> GetIdByName(string name, CancellationToken cancellation);

        Task<string> GetNameById(T id, CancellationToken cancellation);
    }
}