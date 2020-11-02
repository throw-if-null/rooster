using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Adapters.Kudu;
using Rooster.Hosting;
using System.Collections.Generic;

namespace Rooster.SqlServer
{
    public class SqlServerHost : AppHost
    {
        public SqlServerHost(
            IOptionsMonitor<AppHostOptions> options,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<SqlServerHost> logger)
            : base(options, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SqlServerHost)} started.";

        protected override string StopLogMessage => $"{nameof(SqlServerHost)} stopped.";
    }
}
