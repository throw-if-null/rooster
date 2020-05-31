using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.LogEntries;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster
{
    internal class AppHost : IHostedService
    {
        private readonly IKuduApiAdapter _kudu;
        private readonly ILogExtractor _extractor;
        private readonly ILogbookRepository _logbookRepository;
        private readonly ILogEntryRepository _logEntryRepository;
        private readonly IAppServiceRepository _appServiceRepository;

        public AppHost(
            IKuduApiAdapter kudu,
            ILogExtractor extractor,
            ILogbookRepository logbookRepository,
            ILogEntryRepository logEntryRepository,
            IAppServiceRepository appServiceRepository)
        {
            _kudu = kudu ?? throw new ArgumentNullException(nameof(kudu));
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            _logbookRepository = logbookRepository ?? throw new ArgumentNullException(nameof(logbookRepository));
            _logEntryRepository = logEntryRepository ?? throw new ArgumentNullException(nameof(logEntryRepository));
            _appServiceRepository = appServiceRepository ?? throw new ArgumentNullException(nameof(appServiceRepository));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var logbooks = await _kudu.GetLogs(cancellationToken);

            foreach(var logbook in logbooks)
            {
                var latestLogbook = await _logbookRepository.GetLast(cancellationToken);

                if (latestLogbook == null)
                {
                    await _logbookRepository.Create(logbook, cancellationToken);
                    latestLogbook = logbook;
                }

                if (logbook.LastUpdated < latestLogbook.LastUpdated)
                    continue;

                await
                    _kudu.ExtractLogsFromStream(
                        logbook.Href,
                        async (line) =>
                        {
                            var logEntry = _extractor.Extract(line);
                            var latestLogEntry = await _logEntryRepository.GetLatest();

                            if (logEntry.Date <= latestLogEntry)
                                return;

                            logEntry.AppService.Id = await _appServiceRepository.GetIdByName(logEntry.AppService.Name);

                            if (logEntry.AppService.Id == default)
                                logEntry.AppService.Id = await _appServiceRepository.Create(logEntry.AppService.Name);

                            await _logEntryRepository.Create(logEntry);
                        },
                        cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
