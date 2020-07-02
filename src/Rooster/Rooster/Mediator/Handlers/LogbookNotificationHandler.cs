using MediatR;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Notifications;
using Rooster.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class LogbookNotificationHandler<T> : INotificationHandler<LogbookNotification<T>>
    {
        public ILogbookService<T> LogbookService { get; }

        public IAppServiceService<T> AppServiceService { get; }

        public IContainerInstanceService<T> ContainerInstanceService { get; }

        public IKuduApiAdapter<T> Kudu { get; }

        public LogbookNotificationHandler(
            ILogbookService<T> logbookService,
            IAppServiceService<T> appServiceService,
            IContainerInstanceService<T> containerInstanceService,
            IKuduApiAdapter<T> kudu)
        {
            LogbookService = logbookService ?? throw new ArgumentNullException(nameof(logbookService));
            AppServiceService = appServiceService ?? throw new ArgumentNullException(nameof(appServiceService));
            ContainerInstanceService = containerInstanceService ?? throw new ArgumentNullException(nameof(containerInstanceService));
            Kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
        }

        public virtual async Task Handle(LogbookNotification<T> notification, CancellationToken cancellationToken)
        {
            var websiteName = notification.Href.Host.Replace(".scm.azurewebsites.net", string.Empty);
            var appServiceId = await AppServiceService.GetOrAdd(websiteName, cancellationToken);

            notification.ContainerInstanceId = await ContainerInstanceService.GetOrAdd(notification.MachineName, appServiceId, cancellationToken);

            var lastUpdateDate = await LogbookService.GetOrAddIfNewer(notification, cancellationToken);

            if (notification.LastUpdated < lastUpdateDate)
                return;

            await Kudu.ExtractLogsFromStream(notification, cancellationToken);
        }
    }
}