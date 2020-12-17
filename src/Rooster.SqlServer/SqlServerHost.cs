using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Hosting;

namespace Rooster.SqlServer
{
    public class SqlServerHost : AppHost
    {
        public SqlServerHost(
            IMediator mediator,
            ILogger<SqlServerHost> logger)
            : base(mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SqlServerHost)} started.";

        protected override string StopLogMessage => $"{nameof(SqlServerHost)} stopped.";
    }
}
