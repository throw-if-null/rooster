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
                        !x.machineName.EndsWith(DefaultSuffix, StringComparison.InvariantCultureIgnoreCase) &&
                        !x.machineName.EndsWith(MsiSuffix, StringComparison.InvariantCultureIgnoreCase))
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