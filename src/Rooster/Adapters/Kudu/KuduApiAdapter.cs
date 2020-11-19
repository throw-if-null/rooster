using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
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
        Task<IEnumerable<(DateTimeOffset LastUpdated, Uri LogUri, string MachineName)>> GetDockerLogs(CancellationToken cancellation);

        IAsyncEnumerable<string> ExtractLogsFromStream(Uri logUri);
    }

    public class KuduApiAdapter : IKuduApiAdapter
    {
        private const string DefaultSuffix = "_default";
        private const string MsiSuffix = "_msi";
        private const string KuduLogPath = "api/logs/docker";
        private const string LogUrlLogMessage = "Log URL: {0}{1}";

        private static Lazy<JsonSerializerOptions> JsonSerializerOptionInstance =
            new Lazy<JsonSerializerOptions>(
                () => new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                true);

        private readonly HttpClient _client;
        private readonly RecyclableMemoryStreamManager _streamManager;
        private readonly ILogger _logger;

        public KuduApiAdapter(HttpClient client, RecyclableMemoryStreamManager streamManager, ILogger<KuduApiAdapter> logger)
        {
            _client = client;
            _streamManager = streamManager;
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
                JsonSerializerOptionInstance.Value,
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
            using MemoryStream managedStream = _streamManager.GetStream();

            using (var stream = await _client.GetStreamAsync(logUri))
            {
                await stream.CopyToAsync(managedStream);
            }

            var logReader = PipeReader.Create(managedStream);

            while (true)
            {
                var read = await logReader.ReadAsync();
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

                logReader.AdvanceTo(buffer.Start, buffer.End);

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