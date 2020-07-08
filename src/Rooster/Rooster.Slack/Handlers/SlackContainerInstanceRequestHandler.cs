using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.ContainerInstances;
using Rooster.Mediator.Handlers;

namespace Rooster.Slack.Handlers
{
    public class SlackContainerInstanceRequestHandler : ContainerInstanceRequestHandler<object>
    {
        public SlackContainerInstanceRequestHandler(IContainerInstanceRepository<object> containerInstanceRepository, IMemoryCache cache)
            : base(containerInstanceRepository, cache)
        {
        }
    }
}