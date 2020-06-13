using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices.Implementations.Sql
{
    public class SqlAppServiceRepository : ISqlAppServiceRepository
    {
        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[AppService]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"WHERE {propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByName = delegate
        {
            return $"SELECT {nameof(SqlAppService.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(SqlAppService.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(SqlAppService.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(SqlAppService.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO AppService ({nameof(SqlAppService.Name)}) VALUES (@{nameof(SqlAppService.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlAppServiceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<SqlAppService> Create(SqlAppService appService, CancellationToken cancellation)
        {
            _ = appService ?? throw new ArgumentNullException(nameof(appService));

            await using var connection = _connectionFactory.CreateConnection();

            appService.Id =
                await
                    connection.ExecuteAsync(
                        BuildInsert(),
                        new { appService.Name });

            return appService;
        }

        public async Task<SqlAppService> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildGetIdByName(), new { Name = name }, cancellationToken: cancellation);
            var id = await connection.QueryFirstOrDefaultAsync<int>(command);

            return id == default ? null : new SqlAppService { Id = id, Name = name };
        }

        public async Task<string> GetNameById(string id, CancellationToken cancellation)
        {
            if (int.TryParse(id, out var typedId))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildGetNameById(), new { Id = typedId }, cancellationToken: cancellation);
            var name = await connection.QueryFirstOrDefaultAsync<string>(command);

            return name;
        }
    }
}