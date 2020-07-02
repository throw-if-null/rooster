using Rooster.Adapters.Kudu;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.Mediator.Handlers;
using Rooster.Mediator.Notifications;
using Rooster.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Handlers
{
    public class SlackLogbookNotification : LogbookNotification<object>
    {
        public SlackLogbookNotification(Logbook<object> logbook) : base(logbook)
        {
        }
    }

    public class SlackLogbookNotificationHandler : LogbookNotificationHandler<object>
    {
        public SlackLogbookNotificationHandler(
            ILogbookService<object> logbookService,
            IAppServiceService<object> appServiceService,
            IContainerInstanceService<object> containerInstanceService,
            IKuduApiAdapter<object> kudu)
            : base(logbookService, appServiceService, containerInstanceService, kudu)
        {
        }

        public override Task Handle(LogbookNotification<object> notification, CancellationToken cancellationToken)
        {
            return Kudu.ExtractLogsFromStream(notification, cancellationToken);
        }
    }
}