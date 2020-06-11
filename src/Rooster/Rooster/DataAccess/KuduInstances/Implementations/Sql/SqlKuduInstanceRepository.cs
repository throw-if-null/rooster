using Dapper;
using Rooster.Connectors.Sql;
using Rooster.DataAccess.KuduInstances.Entities;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.KuduInstances.Implementations.Sql
{
    public class SqlKuduInstanceRepository : ISqlKuduInstanceRepository
    {
        private static readonly Func<string> BuildFrom = delegate
        {
            return $"FROM [dbo].[KuduInstance]";
        };

        private static readonly Func<string, string> BuildWhere = delegate (string propertyName)
        {
            return $"WHERE {propertyName} = @{propertyName}";
        };

        private static readonly Func<string> BuildGetIdByName = delegate
        {
            return $"SELECT {nameof(SqlKuduInstance.Id)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(SqlKuduInstance.Name))}";
        };

        private static readonly Func<string> BuildGetNameById = delegate
        {
            return $"SELECT {nameof(SqlKuduInstance.Name)} {BuildFrom()} WITH(nolock) {BuildWhere(nameof(SqlKuduInstance.Id))}";
        };

        private readonly Func<string> BuildInsert = delegate
        {
            var query =
                new StringBuilder()
                .AppendLine($"INSERT INTO KuduInstance ({nameof(SqlKuduInstance.Name)}) VALUES (@{nameof(SqlKuduInstance.Name)})")
                .AppendLine("SELECT SCOPE_IDENTITY()")
                .ToString();

            return query;
        };

        private readonly IConnectionFactory _connectionFactory;

        public SqlKuduInstanceRepository(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<SqlKuduInstance> Create(SqlKuduInstance kuduInstance, CancellationToken cancellation)
        {
            _ = kuduInstance ?? throw new ArgumentNullException(nameof(kuduInstance));

            await using var connection = _connectionFactory.CreateConnection();

            var id =
                await
                    connection.ExecuteAsync(
                        BuildInsert(),
                        new { kuduInstance.Name });

            return kuduInstance;
        }

        public async Task<SqlKuduInstance> GetIdByName(string name, CancellationToken cancellation)
        {
            if (string.IsNullOrWhiteSpace(name))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var id = await connection.QueryFirstOrDefaultAsync<int>(BuildGetIdByName(), new { Name = name });

            return new SqlKuduInstance { Id = id, Name = name };
        }

        public async Task<string> GetNameById(string id, CancellationToken cancellation)
        {
            if (int.TryParse(id, out var typedId))
                return default;

            await using var connection = _connectionFactory.CreateConnection();

            var name = await connection.QueryFirstOrDefaultAsync<string>(BuildGetNameById(), new { Id = id });

            return name;
        }
    }
}