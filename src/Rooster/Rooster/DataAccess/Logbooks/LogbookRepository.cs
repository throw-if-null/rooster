using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public interface ILogbookRepository
    {
        Task Create(Logbook logbook, CancellationToken cancellation);

        Task<Logbook> GetLast(CancellationToken cancellation);
    }

    public class LogbookRepository : ILogbookRepository
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            return $"{prefix}{nameof(Logbook.MachineName)}, {prefix}{nameof(Logbook.LastUpdated)}, {prefix}{nameof(Logbook.KuduInstanceId)}";
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
            return $"{nameof(Logbook.Id)}, {BuildInsertPropertyList()}";
        };

        public static readonly Func<string> InsertLogbook =
            delegate
            {
                return
                    $"INSERT INTO {nameof(Logbook)} ({BuildInsertPropertyList()}) VALUES({BuildInsertValuesList()})";
            };

        public static readonly Func<string> GetLatestLogbook =
            delegate
            {
                return
                    $"SELECT TOP 1 {BuildGetLatestList()} FROM {nameof(Logbook)} ORDER BY {nameof(Logbook.Created)} DESC";
            };

        private readonly ISqlConnectionFactory _connectionFactory;

        public LogbookRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(Logbook logbook, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(InsertLogbook(), new { logbook.MachineName, logbook.LastUpdated, logbook.KuduInstanceId });
        }

        public async Task<Logbook> GetLast(CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var logbook = await connection.QueryFirstOrDefaultAsync<Logbook>(GetLatestLogbook());

            return logbook;
        }
    }
}
