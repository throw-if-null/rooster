using Dapper;
using Rooster.DataAccess.Logbooks;
using Rooster.DataAccess.Logbooks.Entities;
using Rooster.SqlServer.Connectors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.DataAccess.Logbooks
{
    public class SqlLogbookRepository : LogbookRepository<int>
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            return $"{prefix}{nameof(Logbook<int>.LastUpdated)}, {prefix}{nameof(Logbook<int>.ContainerInstanceId)}";
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

        public static readonly Func<string> GetLatestLastUpdateForContainerInstance =
            delegate
            {
                return
                    $"SELECT TOP 1 {nameof(Logbook<int>.LastUpdated)} FROM {nameof(Logbook<int>)} WHERE " +
                    $"{nameof(Logbook<int>.ContainerInstanceId)} = @{nameof(Logbook<int>.ContainerInstanceId)} " +
                    $"ORDER BY {nameof(Logbook<int>.Created)} DESC";
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
                new { logbook.LastUpdated, logbook.ContainerInstanceId },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);
        }

        protected override async Task<DateTimeOffset> GetLastUpdatedDateForContainerInstanceImplementation(
            int containerInstanceId,
            CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLatestLastUpdateForContainerInstance(),
                new { ContainerInstanceId = containerInstanceId },
                cancellationToken: cancellation);

            var logbook = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return logbook;
        }
    }
}