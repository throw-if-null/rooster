using MediatR;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.Mediator.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Handlers
{
    public abstract class LogbookRequestHandler<T> : IRequestHandler<LogbookRequest<T>, DateTimeOffset>
    {
        private readonly ILogbookRepository<T> _logbookRepository;

        protected LogbookRequestHandler(ILogbookRepository<T> logbookRepository)
        {
            _logbookRepository = logbookRepository ?? throw new ArgumentNullException(nameof(logbookRepository));
        }

        public virtual async Task<DateTimeOffset> Handle(LogbookRequest<T> request, CancellationToken cancellationToken)
        {
            var logbook = new Logbook<T>
            {
                ContainerInstanceId = request.ContainerInstanceId,
                LastUpdated = request.LastUpdated,
                MachineName = request.MachineName
            };

            var lastUpdateDate =
                await
                    _logbookRepository.GetLastUpdatedDateForContainerInstance(
                        logbook.ContainerInstanceId,
                        cancellationToken);

            if (lastUpdateDate == default || lastUpdateDate < logbook.LastUpdated)
            {
                await _logbookRepository.Create(logbook, cancellationToken);

                lastUpdateDate = logbook.LastUpdated;
            }

            return lastUpdateDate;
        }
    }
}