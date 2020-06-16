using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;

namespace Rooster.SqlServer.Connectors
{
    public interface IConnectionFactory
    {
        SqlConnection CreateConnection();
    }

    public class ConnectionFactory : IConnectionFactory
    {
        private static readonly Func<string, SqlConnection> Create = delegate (string connectionString)
        {
            return new SqlConnection(connectionString);
        };

        private readonly ConnectionFactoryOptions _options;

        public ConnectionFactory(IOptionsMonitor<ConnectionFactoryOptions> options)
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