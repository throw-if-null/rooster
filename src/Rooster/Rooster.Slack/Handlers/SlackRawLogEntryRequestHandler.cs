using Rooster.CrossCutting;
using Rooster.Mediator.Handlers;

namespace Rooster.Slack.Handlers
{
    public class SlackRawLogEntryRequestHandler : RawLogEntryRequestHandler<object>
    {
        public SlackRawLogEntryRequestHandler(ILogExtractor extractor)
            : base(extractor)
        {
        }
    }
}