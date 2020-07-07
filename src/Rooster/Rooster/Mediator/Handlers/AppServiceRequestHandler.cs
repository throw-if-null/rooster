using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class AppServiceRequestHandler<T> : IRequestHandler<AppServiceRequest<T>, T>
    {
        private const string KuduSubdomain = ".scm.azurewebsites.net";

        private readonly IAppServiceRepository<T> _appServiceRepository;
        private readonly IMemoryCache _cache;

        protected AppServiceRequestHandler(IAppServiceRepository<T> appServiceRepository, IMemoryCache cache)
        {
            _appServiceRepository = appServiceRepository ?? throw new ArgumentNullException(nameof(appServiceRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public virtual async Task<T> Handle(AppServiceRequest<T> request, CancellationToken cancellationToken)
        {
            var websiteName = request.KuduLogUri.Host.Replace(KuduSubdomain, string.Empty);

            if (_cache.TryGetValue<T>(websiteName, out var appServiceId))
                return appServiceId;

            appServiceId = await _appServiceRepository.GetIdByName(websiteName, cancellationToken);

            if (_appServiceRepository.IsDefaultValue(appServiceId))
                appServiceId = await _appServiceRepository.Create(new AppService<T> { Name = websiteName }, cancellationToken);

            _cache.Set(websiteName, appServiceId, TimeSpan.FromMinutes(10));

            return appServiceId;
        }
    }
}