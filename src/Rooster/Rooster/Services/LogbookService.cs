using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Services
{
    public interface ILogbookService<T>
    {
        Task<DateTimeOffset> GetOrAddIfNewer(Logbook<T> logbook, CancellationToken cancellation);
    }

    public class LogbookService<T> : ILogbookService<T>
    {
        private readonly ILogbookRepository<T> _logbookRepository;

        public LogbookService(ILogbookRepository<T> logbookRepository)
        {
            _logbookRepository = logbookRepository ?? throw new ArgumentNullException(nameof(logbookRepository));
        }

        public async Task<DateTimeOffset> GetOrAddIfNewer(Logbook<T> logbook, CancellationToken cancellation)
        {
            var lastUpdateDate =
                await
                    _logbookRepository.GetLastUpdatedDateForContainerInstance(
                        logbook.ContainerInstanceId,
                        cancellation);

            if (lastUpdateDate == default || lastUpdateDate < logbook.LastUpdated)
            {
                await _logbookRepository.Create(logbook, cancellation);

                lastUpdateDate = logbook.LastUpdated;
            }

            return lastUpdateDate;
        }
    }
}