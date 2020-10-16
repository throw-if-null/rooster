using MediatR;
using Microsoft.Extensions.Hosting;
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
            IHostApplicationLifetime lifetime,
            IEnumerable<IKuduApiAdapter> kudus,
            IMediator mediator,
            ILogger<SqlServerHost> logger)
            : base(options, lifetime, kudus, mediator, logger)
        {
        }

        protected override string StartLogMessage => $"{nameof(SqlServerHost)} started.";

        protected override string StopLogMessage => $"{nameof(SqlServerHost)} stopped.";
    }
}
