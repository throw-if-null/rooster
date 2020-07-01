using Rooster.Adapters.Kudu;
using Rooster.Handlers;
using Rooster.Services;

namespace Rooster.SqlServer.Handlers
{
    public class SqlLogbookNotificationHandler : LogbookNotificationHandler<int>
    {
        public SqlLogbookNotificationHandler(
            ILogbookService<int> logbookService,
            IAppServiceService<int> appServiceService,
            IContainerInstanceService<int> containerInstanceService,
            IKuduApiAdapter<int> kudu)
            : base(logbookService, appServiceService, containerInstanceService, kudu)
        {
        }
    }
}