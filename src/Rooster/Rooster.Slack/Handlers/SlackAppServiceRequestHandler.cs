using Microsoft.Extensions.Caching.Memory;
using Rooster.DataAccess.AppServices;
using Rooster.Mediator.Handlers;

namespace Rooster.Slack.Handlers
{
    public class SlackAppServiceRequestHandler : AppServiceRequestHandler<object>
    {
        public SlackAppServiceRequestHandler(IAppServiceRepository<object> appServiceRepository, IMemoryCache cache)
            : base(appServiceRepository, cache)
        {
        }
    }
}