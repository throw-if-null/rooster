using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Benchmarks.KuduApiAdapter
{
    [MemoryDiagnoser]
    public class KuduApiAdapterBenchmarks
    {
        static readonly Func<AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate ()
            {
                var user = "$bf-studioapi-sandbox";
                var password = "vuicJaYWb9lzK1snu0mD1my8SJ6mkAofemR4LwtCcwmMqYqTG6Cplwkd28qH";

                var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                return new AuthenticationHeaderValue("Basic", base64);
            };

        static readonly HttpClient _client = new HttpClient()
        {
            BaseAddress = new Uri("https://bf-studioapi-sandbox.scm.azurewebsites.net/")
        };

        static readonly Uri uri1 = new Uri("https://bf-studioapi-sandbox.scm.azurewebsites.net/api/vfs/LogFiles/2020_11_16_RD0003FFDB5B85_docker.log");
        static readonly Uri uri2 = new Uri("https://bf-studioapi-sandbox.scm.azurewebsites.net/api/vfs/LogFiles/2020_11_09_RD0003FFDB676A_docker.log");
        static readonly Uri uri3 = new Uri("https://bf-studioapi-sandbox.scm.azurewebsites.net/api/vfs/LogFiles/2020_11_16_RD0003FFDBD625_docker.log");
        static readonly Uri uri4 = new Uri("https://bf-studioapi-sandbox.scm.azurewebsites.net/api/vfs/LogFiles/2020_11_09_RD0003FFDBDE23_docker.log");

        [Benchmark]
        public async Task ExtractLogsFromStream()
        {
            _client.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader();

            var adapter = new KuduApiAdapterOld(_client, NullLogger<KuduApiAdapterOld>.Instance);

            await foreach(var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri3))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri4))
                _ = log;
        }

        [Benchmark]
        public async Task ExtractLogsFromStream2()
        {
            _client.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader();

            var adapter = new Rooster.Adapters.Kudu.KuduApiAdapter(_client, NullLogger<Rooster.Adapters.Kudu.KuduApiAdapter>.Instance);

            await foreach (var log in adapter.ExtractLogsFromStream(uri1))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri2))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri3))
                _ = log;

            await foreach (var log in adapter.ExtractLogsFromStream(uri4))
                _ = log;
        }
    }
}
