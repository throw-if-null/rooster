using Dapper;
using Rooster.DataAccess.ContainerInstances;
using Rooster.DataAccess.ContainerInstances.Entities;
using Rooster.SqlServer.Connectors;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.DataAccess.ContainerInstances
{
    public class SqlContainerInstanceRepository : ContainerInstanceRepository<int>
    {
        private static readonly string TableName = nameof(ContainerInstance<int>);

        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[{TableName}]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"{propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByNameAndAppServiceId = delegate
        {
            return $"SELECT {nameof(ContainerInstance<int>.Id)} {BuildFrom()} WITH(nolock) WHERE {BuildWhere(nameof(ContainerInstance<int>.Name))} AND {BuildWhere(nameof(ContainerInstance<int>.AppServiceId))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(ContainerInstance<int>.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(ContainerInstance<int>.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO {TableName} ({nameof(ContainerInstance<int>.Name)}, {nameof(ContainerInstance<int>.AppServiceId)}) VALUES (@{nameof(ContainerInstance<int>.Name)}, @{nameof(ContainerInstance<int>.AppServiceId)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlContainerInstanceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<int> CreateImplementation(ContainerInstance<int> kuduInstance, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                BuildInsert(),
                new { kuduInstance.Name, kuduInstance.AppServiceId },
                cancellationToken: cancellation);

            kuduInstance.Id = await connection.ExecuteAsync(command);

            return kuduInstance.Id;
        }

        protected override async Task<int> GetIdByNameAndAppServiceIdImplementation(string name, int appServiceId, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(
                BuildGetIdByNameAndAppServiceId(),
                new { Name = name, AppServiceId = appServiceId },
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

        public override bool IsDefaultValue(int value)
        {
            return value == default;
        }
    }
}