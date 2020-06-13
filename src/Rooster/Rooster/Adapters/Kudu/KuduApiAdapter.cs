using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Adapters.Kudu
{
    public interface IKuduApiAdapter<T>
    {
        Task<IEnumerable<Logbook<T>>> GetLogs(CancellationToken cancellation);

        Task ExtractLogsFromStream(Uri logUri, CancellationToken cancellation, Func<string, CancellationToken, Task> persistLogLine);
    }

    public class KuduApiAdapter<T> : IKuduApiAdapter<T>
    {
        private static readonly Func<string, string, AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate (string user, string password)
            {
                if (string.IsNullOrWhiteSpace(user))
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                return
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}")));
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

        public async Task<IEnumerable<T>> GetLogs(CancellationToken cancellation)
        {
            using var response = await _client.GetAsync("api/logs/docker", cancellation);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var logs = JsonConvert.DeserializeObject<IEnumerable<T>>(content);

            return logs;
        }

        public async Task ExtractLogsFromStream(Uri logUrl, CancellationToken cancellation, Func<string, CancellationToken, Task> persistLogLine)
        {
            _ = logUrl ?? throw new ArgumentNullException(nameof(logUrl));
            _ = persistLogLine ?? throw new ArgumentNullException(nameof(persistLogLine));

            Stream stream = null;

            try
            {
                stream = await _client.GetStreamAsync(logUrl);

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

                    await persistLogLine(line, cancellation);
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        Task<IEnumerable<Logbook<T>>> IKuduApiAdapter<T>.GetLogs(CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }
    }
}