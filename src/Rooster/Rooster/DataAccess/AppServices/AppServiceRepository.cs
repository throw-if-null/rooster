using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices
{
    public abstract class AppServiceRepository<T> : IAppServiceRepository<T>
    {
        protected abstract Task<T> CreateImplementation(AppService<T> appService, CancellationToken cancellation);
        protected abstract Task<T> GetIdByNameImplementation(string name, CancellationToken cancellation);
        protected abstract Task<string> GetNameByIdImplementation(T id, CancellationToken cancellation);

        public async Task<T> Create(AppService<T> appService, CancellationToken cancellation)
        {
            _ = appService ?? throw new ArgumentNullException(nameof(appService));

            if (string.IsNullOrWhiteSpace(appService.Name))
                throw new ArgumentException($"{nameof(appService.Name)} is required.");

            appService.Name = appService.Name.Trim().ToLowerInvariant();

            var id = await CreateImplementation(appService, cancellation);

            return id;
        }

        public abstract bool IsDefaultValue(T value);

        public async Task<T> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            name = name.Trim().ToLowerInvariant();

            var id = await GetIdByNameImplementation(name, cancellation);

            return id;
        }

        public async Task<string> GetNameById(T id, CancellationToken cancellation)
        {
            if (IsDefaultValue(id))
                return default;

            var name = await GetNameByIdImplementation(id, cancellation);

            return name;
        }
    }
}