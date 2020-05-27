using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.LogEntries.Entities;
using System;
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
        private const string InsertLogEntryQuery = "INSERT INTO LogEntry () VALUES () WITH(NOLOCK)";

        private readonly ISqlConnectionFactory _connectionFactory;

        public LogEntryRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(LogEntry entry, CancellationToken cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(InsertLogEntryQuery, new { });
        }
    }
}