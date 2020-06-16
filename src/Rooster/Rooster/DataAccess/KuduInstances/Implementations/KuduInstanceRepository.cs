using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances.Implementations
{
    public abstract class KuduInstanceRepository<T> : IKuduInstanceRepository<T>
    {
        protected abstract bool IsDefaultValue(T value);
        protected abstract Task<T> CreateImplementation(KuduInstance<T> kuduInstance, CancellationToken cancellation);
        protected abstract Task<T> GetIdByNameImplementation(string name, CancellationToken cancellation);
        protected abstract Task<string> GetNameByIdImplementation(T id, CancellationToken cancellation);

        public Task<T> Create(KuduInstance<T> kuduInstance, CancellationToken cancellation)
        {
            _ = kuduInstance ?? throw new ArgumentNullException(nameof(kuduInstance));

            if (string.IsNullOrWhiteSpace(kuduInstance.Name))
                throw new ArgumentException($"{nameof(kuduInstance.Name)} is required.");

            return CreateImplementation(kuduInstance, cancellation);
        }

        public Task<T> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            name = name.Trim().ToLowerInvariant();

            return GetIdByNameImplementation(name, cancellation);
        }

        public Task<string> GetNameById(T id, CancellationToken cancellation)
        {
            if (IsDefaultValue(id))
                return default;

            return GetNameByIdImplementation(id, cancellation);
        }
    }
}