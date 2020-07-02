using Rooster.DataAccess.AppServices.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices
{
    public class NullAppServiceRepository : AppServiceRepository<object>
    {
        public override bool IsDefaultValue(object value) => false;

        protected override Task<object> CreateImplementation(AppService<object> appService, CancellationToken cancellation)
        {
            return Task.FromResult<object>(null);
        }

        protected override Task<object> GetIdByNameImplementation(string name, CancellationToken cancellation)
        {
            return Task.FromResult<object>(null);
        }

        protected override Task<string> GetNameByIdImplementation(object id, CancellationToken cancellation)
        {
            return Task.FromResult(string.Empty);
        }
    }
}