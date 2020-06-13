using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.DataAccess.AppServices.Implementations.MongoDb;
using Rooster.DataAccess.KuduInstances.Entities;
using Rooster.DataAccess.KuduInstances.Implementations.MongoDb;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.DataAccess.Logbooks.Implementations.MongoDb;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.DataAccess.LogEntries.Implementations.MongoDb;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.AppHosts
{
    public class MongoDbAppHost : IHostedService
    {
        private readonly AppHostOptions _options;
        private readonly IKuduApiAdapter<ObjectId> _kudu;
        private readonly ILogExtractor _extractor;
        private readonly IMongoDbLogbookRepository _logbookRepository;
        private readonly IMongoDbLogEntryRepository _logEntryRepository;
        private readonly IMongoDbAppServiceRepository _appServiceRepository;
        private readonly IMongoDbKuduInstanceRepository _kuduInstanceRepository;
        private readonly ILogger _logger;

        public MongoDbAppHost(
            IOptionsMonitor<AppHostOptions> options,
            IKuduApiAdapter<ObjectId> kudu,
            ILogExtractor extractor,
            IMongoDbLogbookRepository logbookRepository,
            IMongoDbLogEntryRepository logEntryRepository,
            IMongoDbAppServiceRepository appServiceRepository,
            IMongoDbKuduInstanceRepository kuduInstanceRepository,
            ILogger<MongoDbAppHost> logger)
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

                foreach (MongoDbLogbook logbook in logbooks)
                {
                    var latestLogbook = await _logbookRepository.GetLast(cancellationToken);

                    if (latestLogbook == null)
                    {
                        var kuduInstance = await _kuduInstanceRepository.GetIdByName(logbook.Href.Host, cancellationToken);

                        if (kuduInstance == null)
                        {
                            kuduInstance = new MongoDbKuduInstance { Name = logbook.Href.Host };
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
                appService = await _appServiceRepository.Create(new MongoDbAppService { Name = websiteName }, cancellation);

            var logEntry = new MongoDbLogEntry(
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
