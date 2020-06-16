using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.Logbooks.Entities;
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

        private static Func<string> BuildGetLatestList = delegate ()
        {
            return $"{nameof(Logbook<int>.Id)}, {BuildInsertPropertyList()}";
        };

        public static readonly Func<string> InsertLogbook =
            delegate
            {
                return
                    $"INSERT INTO {nameof(Logbook<int>)} ({BuildInsertPropertyList()}) VALUES({BuildInsertValuesList()})";
            };

        public static readonly Func<string> GetLatestLogbook =
            delegate
            {
                return
                    $"SELECT TOP 1 {BuildGetLatestList()} FROM {nameof(Logbook<int>)} ORDER BY {nameof(Logbook<int>.Created)} DESC";
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

        public override async Task<Logbook<int>> GetLast(CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(GetLatestLogbook(), cancellationToken: cancellation);
            var logbook = await connection.QueryFirstOrDefaultAsync<Logbook<int>>(command);

            return logbook;
        }
    }
}