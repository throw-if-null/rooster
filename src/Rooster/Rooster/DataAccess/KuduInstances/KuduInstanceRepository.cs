using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances
{
    public interface IKuduInstaceRepository
    {
        Task<int> Create(string name);

        Task<int> GetIdByName(string name);

        Task<string> GetNameById(int id);
    }

    public class KuduInstanceRepository : IKuduInstaceRepository
    {
        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[{nameof(KuduInstance)}]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"WHERE {propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByName = delegate
        {
            return $"SELECT {nameof(KuduInstance.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(KuduInstance.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(KuduInstance.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(KuduInstance.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO {nameof(KuduInstance)} ({nameof(KuduInstance.Name)}) VALUES (@{nameof(KuduInstance.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly ISqlConnectionFactory _connectionFactory;

        public KuduInstanceRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<int> Create(string name)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var id =
                await
                    connection.ExecuteAsync(
                        BuildInsert(),
                        new { Name = name });

            return id;
        }

        public async Task<int> GetIdByName(string name)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var id = await connection.QueryFirstOrDefaultAsync<int>(BuildGetIdByName(), new { Name = name });

            return id;
        }

        public async Task<string> GetNameById(int id)
        {
            await using var connection = _connectionFactory.CreateConnection();

            var name = await connection.QueryFirstOrDefaultAsync<string>(BuildGetNameById(), new { Id = id });

            return name;
        }
    }
}