using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static System.Convert;

namespace Rooster.Adapters.Kudu
{
    public interface IKuduApiAdapter
    {
        Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation);

        IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri);
    }

    public class KuduApiAdapter : IKuduApiAdapter
    {
        private static readonly Func<string, string, AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate (string user, string password)
            {
                if (string.IsNullOrWhiteSpace(user))
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                return new AuthenticationHeaderValue("Basic", ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}")));
            };

        private static readonly Func<HttpClient, KuduAdapterOptions, HttpClient> BuildHttpClient =
            delegate (HttpClient client, KuduAdapterOptions options)
            {
                client.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(options.User, options.Password);
                client.BaseAddress = options.BaseUri;

                return client;
            };

        private readonly HttpClient _client;

        public KuduApiAdapter(IOptionsMonitor<KuduAdapterOptions> options, HttpClient client)
        {
            _ = client ?? throw new ArgumentNullException(nameof(client));
            _ = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));

            _client = BuildHttpClient(client, options?.CurrentValue);
        }

        public async Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation)
        {
            using var response = await _client.GetAsync("api/logs/docker", cancellation);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var kuduLog = new[]
            {
                new
                {
                    lastUpdated = DateTimeOffset.UtcNow,
                    href = _client.BaseAddress,
                    machineName = string.Empty
                }
            };

            var logs = JsonConvert.DeserializeAnonymousType(content, kuduLog);

            var values = logs.Select(x => (x.lastUpdated, x.href, x.machineName));

            return values.Where(
                    x =>
                        !x.machineName.EndsWith("_default", StringComparison.InvariantCultureIgnoreCase) &&
                        !x.machineName.EndsWith("_msi", StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
        }

        public async IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri)
        {
            Stream stream = null;

            try
            {
                stream = await _client.GetStreamAsync(logUri);

                using var logReader = new StreamReader(stream);
                stream = null;

                var line = "init";

                while (line != null)
                {
                    line = await logReader.ReadLineAsync();

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    if (!line.Contains("docker", StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    yield return line;
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }
    }
}