using Rooster.CrossCutting;
using Rooster.Mediator.Handlers;

namespace Rooster.SqlServer.Handlers
{
    public class SqlExportLogEntryRequestHandler : ExportLogEntryRequestHandler<int>
    {
        public SqlExportLogEntryRequestHandler(ILogExtractor extractor) : base(extractor)
        {
        }
    }
}