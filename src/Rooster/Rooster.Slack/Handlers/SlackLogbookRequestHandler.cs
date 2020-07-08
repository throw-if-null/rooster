using Rooster.DataAccess.Logbooks;
using Rooster.Mediator.Handlers;

namespace Rooster.Slack.Handlers
{
    public class SlackLogbookRequestHandler : LogbookRequestHandler<object>
    {
        public SlackLogbookRequestHandler(ILogbookRepository<object> logbookRepository)
            : base(logbookRepository)
        {
        }
    }
}
