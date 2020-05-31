using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;

namespace Rooster.Connectors.Sql
{
    public interface ISqlConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private static readonly Func<string, SqlConnection> Create = delegate (string connectionString)
        {
            return new SqlConnection(connectionString);
        };

        private readonly SqlServerConnectionFactoryOptions _options;

        public SqlConnectionFactory(IOptionsMonitor<SqlServerConnectionFactoryOptions> options)
        {
            _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
        }

        public SqlConnection CreateConnection()
        {
            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
                throw new ArgumentException("Connection string must be provided.");

            return Create(_options.ConnectionString);
        }
    }
}