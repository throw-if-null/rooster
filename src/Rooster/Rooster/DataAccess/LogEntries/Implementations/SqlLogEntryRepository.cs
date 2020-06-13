using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries.Implementations
{
    public class SqlLogEntryRepository : ILogEntryRepository<int>
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            var builder = new StringBuilder();

            builder
                .Append($"{prefix}{nameof(LogEntry<int>.AppServiceId)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.ContainerName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.Date)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.HostName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.ImageName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.InboundPort)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.OutboundPort)}");

            return builder.ToString();
        };

        private static readonly Func<string> BuildPropertiesList = delegate
        {
            return BuildList(string.Empty);
        };

        private static readonly Func<string> BuildValuesList = delegate
        {
            return BuildList("@");
        };

        private static readonly Func<string> InsertLogEntryQuery = delegate
        {
            return $"INSERT INTO {nameof(LogEntry<int>)} ({BuildPropertiesList()}) VALUES ({BuildValuesList()})";
        };

        private static readonly Func<string> GetLastLogEntryDate =
            delegate
            {
                return
                    $"SELECT TOP 1 {nameof(LogEntry<int>.Date)} FROM {nameof(LogEntry<int>)} " +
                    $"WHERE {nameof(LogEntry<int>.AppServiceId)} = @{nameof(LogEntry<int>.AppServiceId)} " +
                    $"ORDER BY {nameof(LogEntry<int>.Created)} DESC";
            };

        private readonly IConnectionFactory _connectionFactory;

        public SqlLogEntryRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(LogEntry<int> entry, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogEntryQuery(),
                new
                {
                    entry.AppServiceId,
                    entry.ContainerName,
                    entry.Date,
                    entry.HostName,
                    entry.ImageName,
                    entry.InboundPort,
                    entry.OutboundPort
                },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);
        }

        public async Task<DateTimeOffset> GetLatestForAppService(int appServiceId, CancellationToken cancellation)
        {
            if (appServiceId == default)
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLastLogEntryDate(),
                new { AppServiceId = appServiceId },
                cancellationToken: cancellation);

            var lastDate = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return lastDate;
        }
    }
}