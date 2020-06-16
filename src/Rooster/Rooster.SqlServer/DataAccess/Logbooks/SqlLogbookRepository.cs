using Dapper;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.SqlServer.Connectors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations
{
    public class SqlLogbookRepository : LogbookRepository<int>
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            return $"{prefix}{nameof(Logbook<int>.MachineName)}, {prefix}{nameof(Logbook<int>.LastUpdated)}, {prefix}{nameof(Logbook<int>.KuduInstanceId)}";
        };

        private static readonly Func<string> BuildInsertPropertyList = delegate ()
        {
            return BuildList(string.Empty);
        };

        private static readonly Func<string> BuildInsertValuesList = delegate ()
        {
            return BuildList("@");
        };

        public static readonly Func<string> InsertLogbook =
            delegate
            {
                return
                    $"INSERT INTO {nameof(Logbook<int>)} ({BuildInsertPropertyList()}) VALUES({BuildInsertValuesList()})";
            };

        public static readonly Func<string> GetLatestLastUpdateForKuduInstance =
            delegate
            {
                return
                    $"SELECT TOP 1 {nameof(Logbook<int>.LastUpdated)} FROM {nameof(Logbook<int>)} WHERE {nameof(Logbook<int>.KuduInstanceId)} = @{nameof(Logbook<int>.KuduInstanceId)} ORDER BY {nameof(Logbook<int>.Created)} DESC";
            };

        private readonly IConnectionFactory _connectionFactory;

        public SqlLogbookRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override bool IsDefaultValue(int value)
        {
            return value == default;
        }

        protected override async Task CreateImplementation(Logbook<int> logbook, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogbook(),
                new { logbook.MachineName, logbook.LastUpdated, logbook.KuduInstanceId },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);
        }

        protected override async Task<DateTimeOffset> GetLatestDateForKuduInstanceImplementation(int kuduInstanceId, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLatestLastUpdateForKuduInstance(),
                new { KuduInstanceId = kuduInstanceId },
                cancellationToken: cancellation);

            var logbook = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return logbook;
        }
    }
}