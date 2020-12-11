using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Adapters.Kudu
{
    public interface IKuduApiAdapter
    {
        Uri BaseUrl { get; }

        Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation);

        IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri);
    }

    public class KuduApiAdapter : IKuduApiAdapter
    {
        private const string DefaultSuffix = "_default";
        private const string MsiSuffix = "_msi";
        private const string KuduLogPath = "api/logs/docker";
        private const string LogUrlLogMessage = "Log URL: {0}{1}";

        private static readonly Lazy<JsonSerializerOptions> _jsonSerializerOptionInstance =
            new Lazy<JsonSerializerOptions>(
                () => new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                true);

        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public KuduApiAdapter(HttpClient client, ILogger<KuduApiAdapter> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Uri BaseUrl => _client.BaseAddress;

        public async Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>>
            GetDockerLogs(CancellationToken cancellation)
        {
            _logger.LogDebug(LogUrlLogMessage, new object[2] { _client.BaseAddress, KuduLogPath });

            using var response = await _client.GetAsync(KuduLogPath, cancellation);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellation);

            var logs = await JsonSerializer.DeserializeAsync<KuduLog[]>(
                stream,
                _jsonSerializerOptionInstance.Value,
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
            await using var stream = await _client.GetStreamAsync(logUri);
            var pipeReader = PipeReader.Create(stream);

            while (true)
            {
                var read = await pipeReader.ReadAsync();
                ReadOnlySequence<byte> buffer = read.Buffer;
                ReadOnlySequence<byte> line = default;

                do
                {
                    line = ReadLogLine(ref buffer);
                    var dockerLine = ProcessLogLine(line);

                    if (string.IsNullOrWhiteSpace(dockerLine))
                        continue;

                    yield return dockerLine;
                }
                while (line.Length > 0);

                pipeReader.AdvanceTo(buffer.Start, buffer.End);

                if (read.IsCompleted)
                {
                    break;
                }
            }
        }

        private static ReadOnlySequence<byte> ReadLogLine(ref ReadOnlySequence<byte> buffer)
        {
            var position = buffer.PositionOf((byte)'\n');

            if (position == null)
                return default;

            var line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return line;
        }

        private static string ProcessLogLine(ReadOnlySequence<byte> sequence)
        {
            Span<char> chars = stackalloc char[(int)sequence.Length];
            Encoding.UTF8.GetChars(sequence, chars);

            return
                chars.IndexOf("docker".AsSpan()) == -1
                ? default
                : chars.ToString();
        }
    }
}