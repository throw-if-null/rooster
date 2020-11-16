using Microsoft.Extensions.Logging;
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
                var logReader = PipeReader.Create(stream);
                stream = null;

                while (true)
                {
                    var read = await logReader.ReadAsync();
                    ReadOnlySequence<byte> buffer = read.Buffer;

                    while (TryReadLine(ref buffer, out ReadOnlySequence<byte> sequence))
                    {
                        var dockerLine = ProcessSequence(sequence);

                        if (dockerLine.Length == 0)
                            continue;

                        yield return dockerLine;
                    }

                    logReader.AdvanceTo(buffer.Start, buffer.End);

                    if (read.IsCompleted)
                        break;
                }
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
        {
            var position = buffer.PositionOf((byte)'\n');

            if (position == null)
            {
                line = default;

                return false;
            }

            line = buffer.Slice(0, position.Value);
            buffer = buffer.Slice(buffer.GetPosition(1, position.Value));

            return true;
        }

        private static string ProcessSequence(ReadOnlySequence<byte> sequence)
        {
            bool isDockerLine = false;

            if (sequence.IsSingleSegment)
            {
                isDockerLine = CheckIfDockerLine(sequence.FirstSpan);

                if (!isDockerLine)
                    return string.Empty;

                return GetCharacters(sequence.FirstSpan).ToString();
            }

            Span<byte> span = stackalloc byte[(int)sequence.Length];
            sequence.CopyTo(span);

            isDockerLine = CheckIfDockerLine(span);

            if (isDockerLine)
            {
                var characters = GetCharacters(span);

                return characters.ToString();
            }

            return string.Empty;
        }

        private static Span<char> GetCharacters(ReadOnlySpan<byte> span)
        {
            Span<char> chars = stackalloc char[span.Length];
            Encoding.UTF8.GetChars(span, chars);

            var copy = new Span<char>(chars.ToArray());
            chars.CopyTo(copy);

            return copy;
        }

        private static Span<char> GetCharacters(Span<byte> span)
        {
            Span<char> chars = stackalloc char[span.Length];
            Encoding.UTF8.GetChars(span, chars);

            var copy = new Span<char>(chars.ToArray());
            chars.CopyTo(copy);

            return copy;
        }

        private static bool CheckIfDockerLine(ReadOnlySpan<byte> bytes)
        {
            Span<char> chars = stackalloc char[bytes.Length];
            Encoding.UTF8.GetChars(bytes, chars);

            if (chars.IndexOf(Docker) >= 0)
                return true;

            return false;
        }
    }
}