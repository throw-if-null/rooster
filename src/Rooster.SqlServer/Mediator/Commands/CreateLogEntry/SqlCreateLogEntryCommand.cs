using Dapper;
using MediatR;
using Rooster.Mediator.Commands.ValidateDockerRunParams;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Schema;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.Mediator.Commands.CreateLogEntry
{
    public sealed class SqlCreateLogEntryCommand : ValidateDockerRunParamsCommand
    {
        private static readonly string TableName = "LogEntry";

        private static readonly Func<string, string> BuildList = delegate (string prefix)
        {
            var builder = new StringBuilder();

            builder
                .Append($"{prefix}{nameof(LogEntry.ServiceName)}, ")
                .Append($"{prefix}{nameof(LogEntry.ContainerName)}, ")
                .Append($"{prefix}{nameof(LogEntry.ImageName)}, ")
                .Append($"{prefix}{nameof(LogEntry.ImageTag)}, ")
                .Append($"{prefix}{nameof(LogEntry.InboundPort)}, ")
                .Append($"{prefix}{nameof(LogEntry.OutboundPort)}, ")
                .Append($"{prefix}{nameof(LogEntry.EventDate)}");

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

        private readonly IConnectionFactory _connectionFactory;

        public SqlCreateLogEntryCommand(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<Unit> CreateImplementation(ValidateDockerRunParamsRequest request, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                InsertLogEntryQuery(),
                new
                {
                    request.ServiceName,
                    request.ContainerName,
                    request.ImageName,
                    request.ImageTag,
                    request.InboundPort,
                    request.OutboundPort,
                    request.EventDate
                },
                cancellationToken: cancellation);

            await connection.ExecuteAsync(command);

            return Unit.Value;
        }
    }
}