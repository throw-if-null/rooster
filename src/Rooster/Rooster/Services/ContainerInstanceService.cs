using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.ContainerInstances;
using Rooster.DataAccess.ContainerInstances.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Services
{
    public interface IContainerInstanceService<T>
    {
        Task<T> GetOrAdd(string machineName, T appServiceId, CancellationToken cancellation);
    }

    public class ContainerInstanceService<T> : IContainerInstanceService<T>
    {
        private readonly IContainerInstanceRepository<T> _containerInstanceRepository;
        private readonly IMemoryCache _cache;

        public ContainerInstanceService(IContainerInstanceRepository<T> containerInstanceRepository, IMemoryCache cache)
        {
            _containerInstanceRepository = containerInstanceRepository ?? throw new ArgumentNullException(nameof(containerInstanceRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<T> GetOrAdd(string machineName, T appServiceId, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(machineName))
                throw new ArgumentNullException(nameof(machineName));

            if (_cache.TryGetValue<T>($"{machineName}-{appServiceId}", out var containerInstanceId))
                return containerInstanceId;

            if (_containerInstanceRepository.IsDefaultValue(appServiceId))
                throw new ArgumentNullException(nameof(appServiceId));

            containerInstanceId = await _containerInstanceRepository.GetIdByNameAndAppServiceId(machineName, appServiceId, cancellation);

            if (_containerInstanceRepository.IsDefaultValue(containerInstanceId))
                containerInstanceId = await _containerInstanceRepository.Create(NewKuduInstance(machineName, appServiceId), cancellation);

            _cache.Set($"{machineName}-{appServiceId}", containerInstanceId, TimeSpan.FromMinutes(10));

            return containerInstanceId;
        }

        private static ContainerInstance<T> NewKuduInstance(string name, T appServiceId) =>
            new ContainerInstance<T> { Name = name, AppServiceId = appServiceId };
    }
}