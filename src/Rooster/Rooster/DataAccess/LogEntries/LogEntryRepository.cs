using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.LogEntries
{
    public interface ILogEntryRepository
    {
        Task Create(LogEntry entry, CancellationToken cancellation);
    }

    public class LogEntryRepository : ILogEntryRepository
    {
        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            var builder = new StringBuilder();

            builder
                .Append($"{prefix}AppServiceId, ")
                .Append($"{prefix}{nameof(LogEntry.ContainerName)}, ")
                .Append($"{prefix}{nameof(LogEntry.Date)}, ")
                .Append($"{prefix}{nameof(LogEntry.HostName)}, ")
                .Append($"{prefix}{nameof(LogEntry.ImageName)}, ")
                .Append($"{prefix}{nameof(LogEntry.InboundPort)}")
                .Append($"{prefix}{nameof(LogEntry.OutbouondPort)}");

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

        private static readonly string InsertLogEntryQuery = $"INSERT INTO {nameof(LogEntry)} ({BuildPropertiesList()}) VALUES ({BuildValuesList()})";

        private readonly ISqlConnectionFactory _connectionFactory;

        public LogEntryRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(LogEntry entry, CancellationToken cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                InsertLogEntryQuery,
                new
                {
                    entry.AppService.Id,
                    entry.ContainerName,
                    entry.Date,
                    entry.HostName,
                    entry.ImageName,
                    entry.InboundPort,
                    entry.OutbouondPort
                });
        }
    }
}