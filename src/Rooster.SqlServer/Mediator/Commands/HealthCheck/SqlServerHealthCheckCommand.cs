using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.QoS.Resilency;
using Rooster.SqlServer.Connectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.Mediator.Commands.HealthCheck
{
    public class SqlServerHealthCheckCommand : HealthCheckCommand
    {
        private const string HealthQuery = "SELECT 1";
        private static readonly int[] TransientErrorNumbers = new int[] { 4060, 40197, 40501, 40613, 49918, 49919, 49920, 11001 };

        private readonly IRetryProvider _retryProvider;
        private readonly IConnectionFactory _factory;
        private readonly ILogger _logger;

        public SqlServerHealthCheckCommand(
            IRetryProvider retryProvider,
            IConnectionFactory factory,
            ILogger<SqlServerHealthCheckCommand> logger)
        {
            _retryProvider = retryProvider ?? throw new ArgumentNullException(nameof(retryProvider));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await
                    _retryProvider.RetryOn(
                        (SqlException ex) => TransientErrorNumbers.Contains(ex.Number),
                        _ => false,
                        () => Execute(_factory, HealthQuery, cancellationToken));

                return Healthy(Engines.SqlServer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HealthCheck failed.", Array.Empty<object>());

                return Unhealthy(Engines.SqlServer, ex.ToString());
            }
        }

        private static async Task<int> Execute(IConnectionFactory factory, string query, CancellationToken cancellationToken)
        {
            using (var connection = factory.CreateConnection())
            {
                await connection.OpenAsync(cancellationToken);

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;

                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }
    }
}
