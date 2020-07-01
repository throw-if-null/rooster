using MediatR;
using Rooster.Adapters.Kudu;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Handlers
{
    public class LogbookNotification<T> : Logbook<T>, INotification
    {
        public LogbookNotification(Logbook<T> logbook)
        {
            Id = logbook.Id;
            ContainerInstanceId = logbook.ContainerInstanceId;
            Created = logbook.Created;
            Href = logbook.Href;
            LastUpdated = logbook.LastUpdated;
            MachineName = logbook.MachineName;
            Path = logbook.Path;
            Size = logbook.Size;
        }
    }

    public abstract class LogbookNotificationHandler<T> : INotificationHandler<LogbookNotification<T>>
    {
        private readonly ILogbookService<T> _logbookService;
        private readonly IAppServiceService<T> _appServiceService;
        private readonly IContainerInstanceService<T> _containerInstanceService;
        private readonly IKuduApiAdapter<T> _kudu;

        public LogbookNotificationHandler(
            ILogbookService<T> logbookService,
            IAppServiceService<T> appServiceService,
            IContainerInstanceService<T> containerInstanceService,
            IKuduApiAdapter<T> kudu)
        {
            _logbookService = logbookService ?? throw new ArgumentNullException(nameof(logbookService));
            _appServiceService = appServiceService ?? throw new ArgumentNullException(nameof(appServiceService));
            _containerInstanceService = containerInstanceService ?? throw new ArgumentNullException(nameof(containerInstanceService));
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
        }

        public async Task Handle(LogbookNotification<T> notification, CancellationToken cancellationToken)
        {
            var websiteName = notification.Href.Host.Replace(".scm.azurewebsites.net", string.Empty);
            var appServiceId = await _appServiceService.GetOrAdd(websiteName, cancellationToken);

            notification.ContainerInstanceId = await _containerInstanceService.GetOrAdd(notification.MachineName, appServiceId, cancellationToken);

            var lastUpdateDate = await _logbookService.GetOrAddIfNewer(notification, cancellationToken);

            if (notification.LastUpdated < lastUpdateDate)
                return;

            await _kudu.ExtractLogsFromStream(notification, cancellationToken);
        }
    }
}
