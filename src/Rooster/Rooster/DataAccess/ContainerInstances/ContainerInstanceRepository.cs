using Rooster.DataAccess.ContainerInstances.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.ContainerInstances
{
    public abstract class ContainerInstanceRepository<T> : IContainerInstanceRepository<T>
    {
        protected abstract Task<T> CreateImplementation(ContainerInstance<T> kuduInstance, CancellationToken cancellation);
        protected abstract Task<T> GetIdByNameAndAppServiceIdImplementation(string name, T appServiceId, CancellationToken cancellation);
        protected abstract Task<string> GetNameByIdImplementation(T id, CancellationToken cancellation);

        public Task<T> Create(ContainerInstance<T> kuduInstance, CancellationToken cancellation)
        {
            _ = kuduInstance ?? throw new ArgumentNullException(nameof(kuduInstance));

            if (string.IsNullOrWhiteSpace(kuduInstance.Name))
                throw new ArgumentException($"{nameof(kuduInstance.Name)} is required.");

            return CreateImplementation(kuduInstance, cancellation);
        }

        public abstract bool IsDefaultValue(T value);

        public Task<T> GetIdByNameAndAppServiceId(string name, T appServiceId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            if (IsDefaultValue(appServiceId))
                return default;

            name = name.Trim().ToLowerInvariant();

            return GetIdByNameAndAppServiceIdImplementation(name, appServiceId, cancellation);
        }

        public Task<string> GetNameById(T id, CancellationToken cancellation)
        {
            if (IsDefaultValue(id))
                return default;

            return GetNameByIdImplementation(id, cancellation);
        }
    }
}