using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.AppServices.Entities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Rooster.DataAccess.AppServices
{
    public interface IAppServiceRepository
    {
        Task<int> Create(string name);
        Task<int> GetIdByName(string name);
        Task<string> GetNameById(int id);
    }

    public class AppServiceRepository : IAppServiceRepository
    {
        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[{nameof(AppService)}]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"WHERE {propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByName = delegate
        {
            return $"SELECT {nameof(AppService.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(AppService.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(AppService.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(AppService.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO {nameof(AppService)} ({nameof(AppService.Name)}) VALUES (@{nameof(AppService.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly ISqlConnectionFactory _connectionFactory;

        public AppServiceRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<int> Create(string name)
        {
            using var connection = _connectionFactory.CreateConnection();

            var id =
                await
                    connection.ExecuteAsync(
                        BuildInsert(),
                        new { Name = name });

            return id;
        }

        public async Task<int> GetIdByName(string name)
        {
            using var connection = _connectionFactory.CreateConnection();

            var id = await connection.QueryFirstOrDefaultAsync<int>(BuildGetIdByName(), new { Name = name });

            return id;
        }

        public async Task<string> GetNameById(int id)
        {
            using var connection = _connectionFactory.CreateConnection();

            var name = await connection.QueryFirstOrDefaultAsync<string>(BuildGetNameById(), new { Id = id });

            return name;
        }
    }
}