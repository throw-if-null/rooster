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
        private static readonly string TableName = nameof(LogEntry<int>);

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
            return $"INSERT INTO {TableName} ({BuildPropertiesList()}) VALUES ({BuildValuesList()})";
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
            Validate(entry);

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogEntryQuery(),
                new
                {
                    AppServiceId = entry.AppServiceId,
                    ContainerName = entry.ContainerName.Trim().ToLowerInvariant(),
                    Date = entry.Date,
                    HostName = entry.HostName.Trim().ToLowerInvariant(),
                    ImageName = entry.ImageName.Trim().ToLowerInvariant(),
                    InboundPort = entry.InboundPort,
                    OutboundPort = entry.OutboundPort
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

        private static void Validate(LogEntry<int> logEntry)
        {
            _ = logEntry ?? throw new ArgumentNullException(nameof(logEntry));

            if (logEntry.AppServiceId == default)
                ThrowArgumentException(nameof(logEntry.AppServiceId), logEntry.AppServiceId.ToString());

            if (string.IsNullOrWhiteSpace(logEntry.ContainerName))
                ThrowArgumentException(nameof(logEntry.ContainerName), logEntry.ContainerName == null ? "NULL" : "EMPTY");

            if (logEntry.Date == default || logEntry.Date == DateTimeOffset.MaxValue)
                ThrowArgumentException(nameof(logEntry.Date), logEntry.Date.ToString());

            if (string.IsNullOrWhiteSpace(logEntry.HostName))
                ThrowArgumentException(nameof(logEntry.HostName), logEntry.HostName == null ? "NULL" : "EMPTY");

            if (string.IsNullOrWhiteSpace(logEntry.ImageName))
                ThrowArgumentException(nameof(logEntry.ImageName), logEntry.ImageName == null ? "NULL" : "EMPTY");

            if (logEntry.InboundPort == default)
                ThrowArgumentException(nameof(logEntry.InboundPort), logEntry.InboundPort);

            if (logEntry.OutboundPort == default)
                ThrowArgumentException(nameof(logEntry.OutboundPort), logEntry.InboundPort);
        }

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };
    }
}