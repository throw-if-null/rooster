using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Adapters.Kudu
{
    public interface IKuduApiAdapter
    {
        Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation);

        IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri);
    }

    public class KuduApiAdapter : IKuduApiAdapter
    {
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public KuduApiAdapter(HttpClient client, ILogger<KuduApiAdapter> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation)
        {
            _logger.LogDebug($"Log url: {_client.BaseAddress}api/logs/docker");

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