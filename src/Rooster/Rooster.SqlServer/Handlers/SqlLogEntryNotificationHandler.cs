using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;

namespace Rooster.SqlServer.Handlers
{
    public class SqlLogEntryNotificationHandler : LogEntryNotificationHandler<int>
    {
        public SqlLogEntryNotificationHandler(
            ILogEntryRepository<int> logEntryRepository,
            ILogExtractor extractor)
            : base(logEntryRepository, extractor)
        {
        }
    }
}