using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices.Implementations
{
    public class SqlAppServiceRepository : IAppServiceRepository<int>
    {
        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[{nameof(AppService<int>)}]";
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
                .AppendLine($"INSERT INTO {nameof(AppService<int>)} ({nameof(AppService<int>.Name)}) VALUES (@{nameof(AppService<int>.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlAppServiceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<int> Create(AppService<int> appService, CancellationToken cancellation)
        {
            _ = appService ?? throw new ArgumentNullException(nameof(appService));

            await using var connection = _connectionFactory.CreateConnection();

            var id =
                await
                    connection.ExecuteAsync(
                        BuildInsert(),
                        new { appService.Name });

            return id;
        }

        public async Task<int> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildGetIdByName(), new { Name = name }, cancellationToken: cancellation);
            var id = await connection.QueryFirstOrDefaultAsync<int>(command);

            return id;
        }

        public async Task<string> GetNameById(int id, CancellationToken cancellation)
        {
            if (id == default)
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var command = new CommandDefinition(BuildGetNameById(), new { Id = id }, cancellationToken: cancellation);
            var name = await connection.QueryFirstOrDefaultAsync<string>(command);

            return name;
        }
    }
}