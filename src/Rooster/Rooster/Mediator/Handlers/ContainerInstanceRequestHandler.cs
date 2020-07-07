using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.ContainerInstances;
using Rooster.DataAccess.ContainerInstances.Entities;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class ContainerInstanceRequestHandler<T> : IRequestHandler<ContainerInstanceRequest<T>, T>
    {
        private readonly IContainerInstanceRepository<T> _containerInstanceRepository;
        private readonly IMemoryCache _cache;

        protected ContainerInstanceRequestHandler(IContainerInstanceRepository<T> containerInstanceRepository, IMemoryCache cache)
        {
            _containerInstanceRepository = containerInstanceRepository ?? throw new ArgumentNullException(nameof(containerInstanceRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public virtual async Task<T> Handle(ContainerInstanceRequest<T> request, CancellationToken cancellationToken)
        {
            var machineName = request.MachineName;
            var appServiceId = request.AppServiceId;

            if (_cache.TryGetValue<T>($"{machineName}-{appServiceId}", out var containerInstanceId))
                return containerInstanceId;

            if (_containerInstanceRepository.IsDefaultValue(appServiceId))
                throw new ArgumentNullException(nameof(appServiceId));

            containerInstanceId = await _containerInstanceRepository.GetIdByNameAndAppServiceId(machineName, appServiceId, cancellationToken);

            if (_containerInstanceRepository.IsDefaultValue(containerInstanceId))
                containerInstanceId = await _containerInstanceRepository.Create(NewKuduInstance(machineName, appServiceId), cancellationToken);

            _cache.Set($"{machineName}-{appServiceId}", containerInstanceId, TimeSpan.FromMinutes(10));

            return containerInstanceId;
        }

        private static ContainerInstance<T> NewKuduInstance(string name, T appServiceId) =>
            new ContainerInstance<T> { Name = name, AppServiceId = appServiceId };
    }
}