using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Hosting;

namespace Rooster.Mock
{
    public class MockHost : AppHost
    {
        public MockHost(
            IMediator mediator,
            ILogger<MockHost> logger)
            : base(mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(MockHost)} started.";
        protected override string StopLogMessage => $"{nameof(MockHost)} stopped.";
    }
}
