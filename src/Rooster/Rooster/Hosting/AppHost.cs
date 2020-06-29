using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
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
        private readonly ILogExtractor _extractor;
        private readonly ILogbookService<T> _logbookService;
        private readonly ILogEntryRepository<T> _logEntryRepository;
        private readonly IAppServiceRepository<T> _appServiceRepository;
        private readonly IContainerInstanceService<T> _containerInstanceService;
        private readonly ILogger _logger;

        public AppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter<T> kudu,
            ILogExtractor extractor,
            ILogbookService<T> logbookService,
            ILogEntryRepository<T> logEntryRepository,
            IAppServiceRepository<T> appServiceRepository,
            IContainerInstanceService<T> containerInstanceService,
            ILogger<AppHost<T>> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            _logbookService = logbookService?? throw new ArgumentNullException(nameof(logbookService));
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            _appServiceRepository = appServiceRepository ?? throw new ArgumentNullException(nameof(appServiceRepository));
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
                    var appServiceId = await _appServiceRepository.GetIdByName(websiteName, cancellationToken);

                    if (_appServiceRepository.IsDefaultValue(appServiceId))
                        appServiceId = await _appServiceRepository.Create(new AppService<T> { Name = websiteName }, cancellationToken);

                    logbook.ContainerInstanceId = await _containerInstanceService.GetOrAdd(logbook.MachineName, appServiceId, cancellationToken);

                    var lastUpdateDate = await _logbookService.GetOrAddIfNewer(logbook, cancellationToken);

                    if (logbook.LastUpdated < lastUpdateDate)
                        continue;

                    await _kudu.ExtractLogsFromStream(logbook, cancellationToken, ExtractAndPersistDockerLogLine);
                }

                if (!_options.UseInternalPoller)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));
            }
        }

        private async Task ExtractAndPersistDockerLogLine(string line, T containerInstanceId, CancellationToken cancellation)
        {
            var (inboundPort, outboundPort) = _extractor.ExtractPorts(line);

            var logEntry = new LogEntry<T>(
                containerInstanceId,
                _extractor.ExtractImageName(line),
                _extractor.ExtractContainerName(line),
                inboundPort,
                outboundPort,
                _extractor.ExtractDate(line));

            var latestLogEntry = await _logEntryRepository.GetLatestForLogbook(logEntry.LogbookId, cancellation);

            if (logEntry.Date <= latestLogEntry)
                return;

            await _logEntryRepository.Create(logEntry, cancellation);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
