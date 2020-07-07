using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;

namespace Rooster.SqlServer.Handlers
{
    public class SqlLogEntryRequestHandler : LogEntryRequestHandler<int>
    {
        public SqlLogEntryRequestHandler(ILogEntryRepository<int> logEntryRepository)
            : base(logEntryRepository)
        {
        }
    }
}