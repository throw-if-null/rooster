using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
        private static Func<JsonSerializerOptions> GetJsonSerializerOptions = delegate ()
        {
            return new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        };

        private const string InitStringValue = "init";
        private const string Docker = "docker";
        private const string DefaultSuffix = "_default";
        private const string MsiSuffix = "_msi";
        private const string KuduLogPath = "api/logs/docker";
        private const string LogUrlLogMessage = "Log URL: {0}{1}";

        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public KuduApiAdapter(HttpClient client, ILogger<KuduApiAdapter> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;
        }

        public async Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>>
            GetDockerLogs(CancellationToken cancellation)
        {
            _logger.LogDebug(LogUrlLogMessage, new object[2] { _client.BaseAddress, KuduLogPath });

            using var response = await _client.GetAsync(KuduLogPath, cancellation);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();

            var logs = await JsonSerializer.DeserializeAsync<KuduLog[]>(
                stream,
                GetJsonSerializerOptions(),
                cancellation);

            var values = logs.Where(
                x =>
                    !x.MachineName.EndsWith(DefaultSuffix, StringComparison.InvariantCultureIgnoreCase) &&
                    !x.MachineName.EndsWith(MsiSuffix, StringComparison.InvariantCultureIgnoreCase))
                .Select(x => (x.LastUpdated, x.Href, x.MachineName));

            return values;
        }

        public async IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri)
        {
            Stream stream = null;

            try
            {
                stream = await _client.GetStreamAsync(logUri);

                using var logReader = new StreamReader(stream);
                stream = null;

                var line = InitStringValue;

                while (line != null)
                {
                    line = await logReader.ReadLineAsync();

                    if (!CheckIfDockerRunLine(line))
                        continue;

                    yield return line;
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private static bool CheckIfDockerRunLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            if (!line.Contains(Docker, StringComparison.InvariantCultureIgnoreCase))
                return false;

            return true;
        }
    }
}