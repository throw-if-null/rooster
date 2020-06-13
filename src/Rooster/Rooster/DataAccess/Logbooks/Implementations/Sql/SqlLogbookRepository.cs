using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks.Implementations.Sql
{
    public class SqlLogbookRepository : ISqlLogbookRepository
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            return $"{prefix}{nameof(SqlLogbook.MachineName)}, {prefix}{nameof(SqlLogbook.LastUpdated)}, {prefix}{nameof(SqlLogbook.KuduInstanceId)}";
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
            return $"{nameof(SqlLogbook.Id)}, {BuildInsertPropertyList()}";
        };

        public static readonly Func<string> InsertLogbook =
            delegate
            {
                return
                    $"INSERT INTO Logbook ({BuildInsertPropertyList()}) VALUES({BuildInsertValuesList()})";
            };

        public static readonly Func<string> GetLatestLogbook =
            delegate
            {
                return
                    $"SELECT TOP 1 {BuildGetLatestList()} FROM Logbook ORDER BY {nameof(SqlLogbook.Created)} DESC";
            };

        private readonly IConnectionFactory _connectionFactory;

        public SqlLogbookRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(SqlLogbook logbook, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogbook(),
                new { logbook.MachineName, logbook.LastUpdated, logbook.KuduInstanceId },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);
        }

        public async Task<SqlLogbook> GetLast(CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(GetLatestLogbook(), cancellationToken: cancellation);
            var logbook = await connection.QueryFirstOrDefaultAsync<SqlLogbook>(command);

            return logbook;
        }
    }
}