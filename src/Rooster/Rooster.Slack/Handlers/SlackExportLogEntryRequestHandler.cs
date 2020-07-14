using Rooster.CrossCutting;
using Rooster.Mediator.Handlers;

namespace Rooster.Slack.Handlers
{
    public class SlackExportLogEntryRequestHandler : ExportLogEntryRequestHandler<object>
    {
        public SlackExportLogEntryRequestHandler(ILogExtractor extractor)
            : base(extractor)
        {
        }
    }
}