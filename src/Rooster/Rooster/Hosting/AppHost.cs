using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Hosting
{
    public class AppHost<T> : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IKuduApiAdapter<T> _kudu;
        private readonly ILogbookService<T> _logbookService;
        private readonly IAppServiceService<T> _appServiceService;
        private readonly IContainerInstanceService<T> _containerInstanceService;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter<T> kudu,
            ILogbookService<T> logbookService,
            IAppServiceService<T> appServiceService,
            IContainerInstanceService<T> containerInstanceService,
            ILogger<AppHost<T>> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
            _logbookService = logbookService?? throw new ArgumentNullException(nameof(logbookService));
            _appServiceService = appServiceService ?? throw new ArgumentNullException(nameof(appServiceService));
            _containerInstanceService = containerInstanceService ?? throw new ArgumentNullException(nameof(containerInstanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var logbooks = await _kudu.GetDockerLogs(cancellationToken);

                foreach (var logbook in logbooks)
                {
                    if (logbook.LastUpdated.Date < DateTimeOffset.UtcNow.Date)
                        continue;

                    var websiteName = logbook.Href.Host.Replace(".scm.azurewebsites.net", string.Empty);
                    var appServiceId = await _appServiceService.GetOrAdd(websiteName, cancellationToken);

                    logbook.ContainerInstanceId = await _containerInstanceService.GetOrAdd(logbook.MachineName, appServiceId, cancellationToken);

                    var lastUpdateDate = await _logbookService.GetOrAddIfNewer(logbook, cancellationToken);

                    if (logbook.LastUpdated < lastUpdateDate)
                        continue;

                    await _kudu.ExtractLogsFromStream(logbook, cancellationToken);
                }

                if (!_options.UseInternalPoller)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}