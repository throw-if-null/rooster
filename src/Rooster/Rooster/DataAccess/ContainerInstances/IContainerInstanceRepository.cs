using Rooster.DataAccess.ContainerInstances.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.ContainerInstances
{
    public interface IContainerInstanceRepository<T>
    {
        bool IsDefaultValue(T value) => false;

        Task<T> Create(ContainerInstance<T> kuduInstance, CancellationToken cancellation) => Task.FromResult(default(T));

        Task<T> GetIdByNameAndAppServiceId(string name, T appServiceId, CancellationToken cancellation) => Task.FromResult(default(T));

        Task<string> GetNameById(T id, CancellationToken cancellation) => Task.FromResult("Default");
    }
}