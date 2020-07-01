using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Services
{
    public interface IAppServiceService<T>
    {
        Task<T> GetOrAdd(string websiteName, CancellationToken cancellation);
    }

    public class AppServiceService<T> : IAppServiceService<T>
    {
        private readonly IAppServiceRepository<T> _appServiceRepository;
        private readonly IMemoryCache _cache;
        public AppServiceService(IAppServiceRepository<T> appServiceRepository, IMemoryCache cache)
        {
            _appServiceRepository = appServiceRepository ?? throw new ArgumentNullException(nameof(appServiceRepository));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        public async Task<T> GetOrAdd(string websiteName, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(websiteName))
                throw new ArgumentNullException(nameof(websiteName));

            if (_cache.TryGetValue<T>(websiteName, out var appServiceId))
                return appServiceId;

            appServiceId = await _appServiceRepository.GetIdByName(websiteName, cancellation);

            if (_appServiceRepository.IsDefaultValue(appServiceId))
                appServiceId = await _appServiceRepository.Create(new AppService<T> { Name = websiteName }, cancellation);

            _cache.Set(websiteName, appServiceId, TimeSpan.FromMinutes(10));

            return appServiceId;
        }
    }
}