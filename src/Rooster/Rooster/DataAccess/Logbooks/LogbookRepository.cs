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
        public const string InsertLogbook = "INSERT INTO Logbook (MachineName, LastUpdated) VALUES(@MachineName, @LastUpdated)";
        public const string GetLatestLogbook = "SELECT TOP 1 Id, MachineName, LastUpdated FROM Logbook WITH(nolock) ORDER BY Created DESC";

        private readonly ISqlConnectionFactory _connectionFactory;

        public LogbookRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task Create(Logbook logbook, CancellationToken cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(InsertLogbook, new { logbook.MachineName, logbook.LastUpdated });
        }

        public async Task<Logbook> GetLast(CancellationToken cancellation)
        {
            using var connection = _connectionFactory.CreateConnection();

            var logbook = await connection.QueryFirstOrDefaultAsync<Logbook>(GetLatestLogbook);

            return logbook;
        }
    }
}
