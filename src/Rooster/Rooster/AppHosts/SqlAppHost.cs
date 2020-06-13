using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.DataAccess.AppServices.Implementations.Sql;
using Rooster.DataAccess.KuduInstances.Entities;
using Rooster.DataAccess.KuduInstances.Implementations.Sql;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.DataAccess.Logbooks.Implementations.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.DataAccess.LogEntries.Implementations.Sql;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppHosts
{
    internal class SqlAppHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IKuduApiAdapter<int> _kudu;
        private readonly ILogExtractor _extractor;
        private readonly ISqlLogbookRepository _logbookRepository;
        private readonly ISqlLogEntryRepository _logEntryRepository;
        private readonly ISqlAppServiceRepository _appServiceRepository;
        private readonly ISqlKuduInstanceRepository _kuduInstanceRepository;
        private readonly ILogger _logger;

        public SqlAppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter<int> kudu,
            ILogExtractor extractor,
            ISqlLogbookRepository logbookRepository,
            ISqlLogEntryRepository logEntryRepository,
            ISqlAppServiceRepository appServiceRepository,
            ISqlKuduInstanceRepository kuduInstanceRepository,
            ILogger<SqlAppHost> logger)
        {
            _options = options.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            _logbookRepository = logbookRepository ?? throw new ArgumentNullException(nameof(logbookRepository));
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            _appServiceRepository = appServiceRepository ?? throw new ArgumentNullException(nameof(appServiceRepository));
            _kuduInstanceRepository = kuduInstanceRepository ?? throw new ArgumentNullException(nameof(kuduInstanceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var logbooks = await _kudu.GetLogs(cancellationToken);

                foreach (SqlLogbook logbook in logbooks)
                {
                    var latestLogbook = await _logbookRepository.GetLast(cancellationToken);

                    if (latestLogbook == null)
                    {
                        var kuduInstance = await _kuduInstanceRepository.GetIdByName(logbook.Href.Host, cancellationToken);

                        if (kuduInstance == null)
                        {
                            kuduInstance = new SqlKuduInstance { Name = logbook.Href.Host };
                            kuduInstance = await _kuduInstanceRepository.Create(kuduInstance, cancellationToken);
                        }

                        logbook.KuduInstanceId = kuduInstance.Id;

                        await _logbookRepository.Create(logbook, cancellationToken);
                        latestLogbook = logbook;
                    }

                    if (logbook.LastUpdated < latestLogbook.LastUpdated)
                        continue;

                    await _kudu.ExtractLogsFromStream(logbook.Href, cancellationToken, ExtractAndPersistDockerLogLine);
                }

                await Task.Delay(TimeSpan.FromSeconds(_options.PoolingIntervalInSeconds));
            }
        }

        private async Task ExtractAndPersistDockerLogLine(string line, CancellationToken cancellation)
        {
            var (inboundPort, outboundPort) = _extractor.ExtractPorts(line);

            var websiteName = _extractor.ExtractWebsiteName(line);

            var appService = await _appServiceRepository.GetIdByName(websiteName, cancellation);

            if (appService == null)
                appService = await _appServiceRepository.Create(new SqlAppService { Name = websiteName}, cancellation);

            var logEntry = new SqlLogEntry(
                appService.Id,
                _extractor.ExtractHostName(line),
                _extractor.ExtractImageName(line),
                _extractor.ExtractContainerName(line),
                inboundPort,
                outboundPort,
                _extractor.ExtractDate(line));

            var latestLogEntry = await _logEntryRepository.GetLatestForAppService(logEntry.AppServiceId.ToString(), cancellation);

            if (logEntry.Date <= latestLogEntry)
                return;

            await _logEntryRepository.Create(logEntry, cancellation);

            // TODO: Add Slack integration.
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
