using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances.Implementations
{
    public class SqlKuduInstanceRepository : KuduInstanceRepository<int>
    {
        private static readonly string TableName = nameof(KuduInstance<int>);

        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[{TableName}]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"WHERE {propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByName = delegate
        {
            return $"SELECT {nameof(KuduInstance<int>.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(KuduInstance<int>.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(KuduInstance<int>.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(KuduInstance<int>.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO {TableName} ({nameof(KuduInstance<int>.Name)}) VALUES (@{nameof(KuduInstance<int>.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlKuduInstanceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override bool IsDefaultValue(int value)
        {
            return value == default;
        }

        protected override async Task<int> CreateImplementation(KuduInstance<int> kuduInstance, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                BuildInsert(),
                new { kuduInstance.Name },
                cancellationToken: cancellation);

            kuduInstance.Id = await connection.ExecuteAsync(command);

            return kuduInstance.Id;
        }

        protected override async Task<int> GetIdByNameImplementation(string name, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                BuildGetIdByName(),
                new { Name = name },
                cancellationToken: cancellation);

            var id = await connection.QueryFirstOrDefaultAsync<int>(command);

            return id;
        }

        protected override async Task<string> GetNameByIdImplementation(int id, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildGetNameById(), new { Id = id }, cancellationToken: cancellation);
            var name = await connection.QueryFirstOrDefaultAsync<string>(command);

            return name;
        }
    }
}