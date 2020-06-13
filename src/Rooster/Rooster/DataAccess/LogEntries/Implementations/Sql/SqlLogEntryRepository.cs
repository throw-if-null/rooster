using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries.Implementations.Sql
{
    public class SqlLogEntryRepository : ISqlLogEntryRepository
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            var builder = new StringBuilder();

            builder
                .Append($"{prefix}{nameof(SqlLogEntry.AppServiceId)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.ContainerName)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.Date)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.HostName)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.ImageName)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.InboundPort)}, ")
                .Append($"{prefix}{nameof(SqlLogEntry.OutboundPort)}");

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
            return $"INSERT INTO LogEntry ({BuildPropertiesList()}) VALUES ({BuildValuesList()})";
        };

        private static readonly Func<string> GetLastLogEntryDate =
            delegate
            {
                return
                    $"SELECT TOP 1 {nameof(SqlLogEntry.Date)} FROM LogEntry " +
                    $"WHERE {nameof(SqlLogEntry.AppServiceId)} = @{nameof(SqlLogEntry.AppServiceId)} " +
                    $"ORDER BY {nameof(SqlLogEntry.Created)} DESC";
            };

        private readonly IConnectionFactory _connectionFactory;

        public SqlLogEntryRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(SqlLogEntry entry, CancellationToken cancellation)
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

        public async Task<DateTimeOffset> GetLatestForAppService(string appServiceId, CancellationToken cancellation)
        {
            if (int.TryParse(appServiceId, out var typedAppServiceId))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLastLogEntryDate(),
                new { AppServiceId = typedAppServiceId },
                cancellationToken: cancellation);

            var lastDate = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return lastDate;
        }
    }
}