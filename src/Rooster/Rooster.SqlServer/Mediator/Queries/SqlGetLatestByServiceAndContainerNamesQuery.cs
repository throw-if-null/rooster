using Dapper;
using Rooster.DataAccess.Entities;
using Rooster.Mediator.Queries;
using Rooster.Mediator.Queries.Requests;
using Rooster.SqlServer.Connectors;
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
                       $"SELECT TOP 1 {nameof(LogEntry<int>.EventDate)} FROM {nameof(LogEntry<int>)} " +
                       $"WHERE {nameof(LogEntry<int>.ServiceName)} = @{nameof(LogEntry<int>.ServiceName)} AND " +
                       $"{nameof(LogEntry<int>.ContainerName)} = @{nameof(LogEntry<int>.ContainerName)} " +
                       $"ORDER BY {nameof(LogEntry<int>.Created)} DESC";
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