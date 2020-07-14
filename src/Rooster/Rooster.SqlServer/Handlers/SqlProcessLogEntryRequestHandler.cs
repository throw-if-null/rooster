using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;

namespace Rooster.SqlServer.Handlers
{
    public class SqlProcessLogEntryRequestHandler : ProcessLogEntryRequestHandler<int>
    {
        public SqlProcessLogEntryRequestHandler(ILogEntryRepository<int> logEntryRepository)
            : base(logEntryRepository)
        {
        }
    }
}