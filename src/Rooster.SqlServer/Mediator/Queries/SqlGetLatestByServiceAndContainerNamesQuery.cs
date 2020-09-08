using Dapper;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Schema;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.Mediator.Queries
{
    public sealed class SqlGetLatestByServiceAndContainerNamesQuery : GetLatestByServiceAndContainerNamesQuery
    {
        private static readonly Func<string> GetLastLogEntryDateForAppServiceAndContainer =
               delegate
               {
                   return
                       $"SELECT TOP 1 {nameof(LogEntry.EventDate)} FROM {nameof(LogEntry)} " +
                       $"WHERE {nameof(LogEntry.ServiceName)} = @{nameof(LogEntry.ServiceName)} AND " +
                       $"{nameof(LogEntry.ContainerName)} = @{nameof(LogEntry.ContainerName)} " +
                       $"ORDER BY {nameof(LogEntry.Created)} DESC";
               };

        private readonly IConnectionFactory _connectionFactory;

        public SqlGetLatestByServiceAndContainerNamesQuery(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<DateTimeOffset> GetLatestByServiceAndContainerNamesImplementation(
            GetLatestByServiceAndContainerNamesRequest request,
            CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                GetLastLogEntryDateForAppServiceAndContainer(),
                new { request.ServiceName, request.ContainerName },
                cancellationToken: cancellation);

            var lastDate = await connection.QueryFirstOrDefaultAsync<DateTimeOffset>(command);

            return lastDate;
        }
    }
}