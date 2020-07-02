using Rooster.DataAccess.ContainerInstances.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.ContainerInstances
{
    public class NullContainerInstanceRepository : ContainerInstanceRepository<object>
    {
        public override bool IsDefaultValue(object value) => false;

        protected override Task<object> CreateImplementation(ContainerInstance<object> kuduInstance, CancellationToken cancellation)
        {
            return Task.FromResult<object>(null);
        }

        protected override Task<object> GetIdByNameAndAppServiceIdImplementation(string name, object appServiceId, CancellationToken cancellation)
        {
            return Task.FromResult<object>(null);
        }

        protected override Task<string> GetNameByIdImplementation(object id, CancellationToken cancellation)
        {
            return Task.FromResult(string.Empty);
        }
    }
}