using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IO;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.KuduApiAdapter
{
    [MemoryDiagnoser, NativeMemoryProfiler, ThreadingDiagnoser]
    public class KuduApiAdapterBenchmarks
    {
        static readonly Func<AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate ()
            {
                var user = "$bf-xxx";
                var password = "xxx";

                var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                return new AuthenticationHeaderValue("Basic", base64);
            };

        static readonly HttpClient _client = new HttpClient()
        {
            BaseAddress = new Uri("https://xxx.scm.azurewebsites.net/")
        };

        static readonly Uri uri1 = new Uri("");
        static readonly Uri uri2 = new Uri("");

        static readonly RecyclableMemoryStreamManager _streamManager = new RecyclableMemoryStreamManager();

        [Benchmark]
        public async Task ExtractLogsFromStream()
        {
            _client.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader();

            var adapter = new KuduApiAdapterOld(_client, NullLogger<KuduApiAdapterOld>.Instance);

            await foreach(var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;
        }

        [Benchmark]
        public async Task ExtractLogsFromStream2()
        {
            _client.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader();

            var adapter = new Rooster.Adapters.Kudu.KuduApiAdapter(_client, _streamManager, NullLogger<Rooster.Adapters.Kudu.KuduApiAdapter>.Instance);

            await foreach (var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;
        }
    }
}
