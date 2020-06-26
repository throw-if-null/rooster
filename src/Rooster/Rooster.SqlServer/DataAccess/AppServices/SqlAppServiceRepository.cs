using Dapper;
using Rooster.DataAccess.AppServices;
using Rooster.DataAccess.AppServices.Entities;
using Rooster.SqlServer.Connectors;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.DataAccess.AppServices
{
    public class SqlAppServiceRepository : AppServiceRepository<int>
    {
        private static readonly string TableName = nameof(AppService<int>);

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
            return $"SELECT {nameof(AppService<int>.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(AppService<int>.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(AppService<int>.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(AppService<int>.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO {TableName} ({nameof(AppService<int>.Name)}) VALUES (@{nameof(AppService<int>.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlAppServiceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected override async Task<int> CreateImplementation(AppService<int> appService, CancellationToken cancellation)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildInsert(), new { appService.Name }, cancellationToken: cancellation);
            var id = await connection.ExecuteAsync(command);

            return id;
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

        public override bool IsDefaultValue(int value)
        {
            return value == default;
        }
    }
}