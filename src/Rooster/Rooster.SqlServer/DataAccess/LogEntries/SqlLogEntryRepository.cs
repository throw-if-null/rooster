using Dapper;
using Rooster.DataAccess.LogEntries;
using Rooster.DataAccess.LogEntries.Entities;
using Rooster.SqlServer.Connectors;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.DataAccess.LogEntries
{
    public class SqlLogEntryRepository : LogEntryRepository<int>
    {
        private static readonly string TableName = nameof(LogEntry<int>);

        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            var builder = new StringBuilder();

            builder
                .Append($"{prefix}{nameof(LogEntry<int>.ServiceName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.ContainerName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.ImageName)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.ImageTag)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.InboundPort)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.OutboundPort)}, ")
                .Append($"{prefix}{nameof(LogEntry<int>.EventDate)}");

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

        private static readonly Func<string> GetLastLogEntryDateForAppServiceAndContainer =
            delegate
            {
                return
                    $"SELECT TOP 1 {nameof(LogEntry<int>.EventDate)} FROM {nameof(LogEntry<int>)} " +
                    $"WHERE {nameof(LogEntry<int>.ServiceName)} = @{nameof(LogEntry<int>.ServiceName)} AND " +
                    $"{nameof(LogEntry<int>.ContainerName)} = @{nameof(LogEntry<int>.ContainerName)} " +
                    $"ORDER BY {nameof(LogEntry<int>.Created)} DESC";
            };

        private readonly IConnectionFactory _connectionFactory;

        public SqlLogEntryRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override bool IsDefaultValue(int value)
        {
            return value == default;
        }

        protected override async Task CreateImplementation(LogEntry<int> entry, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogEntryQuery(),
                new
                {
                    entry.ServiceName,
                    entry.ContainerName,
                    entry.ImageName,
                    entry.ImageTag,
                    entry.InboundPort,
                    entry.OutboundPort,
                    entry.EventDate
                },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);
        }

        protected override async Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(string serviceName, string containerName, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLastLogEntryDateForAppServiceAndContainer(),
                new { ServiceName = serviceName, ContainerName = containerName },
                cancellationToken: cancellation);

            var lastDate = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return lastDate;
        }
    }
}