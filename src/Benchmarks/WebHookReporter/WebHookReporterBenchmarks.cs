using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.QoS.Resilency;
using Rooster.Slack.Reporting;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks.WebHookReporter
{
    [MemoryDiagnoser, ThreadingDiagnoser, NativeMemoryProfiler]
    public class WebHookReporterBenchmarks
    {
        private const string message = "New container deployment.";
        private const string DateTitle = "Date";
        private const string ContainerNameTitle = "Container name";
        private const string PortsTitle = "Ports";
        private const string ImageTitle = "Image";
        private const string MarkdownInOption = "text";
        private const string ColorValue = "warning";

        private static ShouldProcessDockerLogRequest request = new ShouldProcessDockerLogRequest
        {
            ExportedLogEntry = new ExtractDockerRunParamsResponse
            {
                ContainerName = "test",
                Created = DateTimeOffset.UtcNow,
                EventDate = DateTimeOffset.UtcNow,
                ImageName = "test",
                ImageTag = "1.0.0",
                InboundPort = 5000.ToString(),
                OutboundPort = 5000.ToString(),
                ServiceName = "test-service"
            }
        };

        private static readonly RecyclableMemoryStreamManager _manager = new RecyclableMemoryStreamManager();

        private CancellationTokenSource _source = new CancellationTokenSource();
        private WebHookReporterOld _reporterOld;
        private Rooster.Slack.Reporting.WebHookReporter _reporter;
        private object _payloadOld;
        private MemoryStream _bytes;

        [GlobalSetup]
        public void Setup()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://hooks.slack.com");

            _reporterOld = new WebHookReporterOld(
                new WebHookReporterOptionsMonitor(),
                client,
                new RetryProvider(new RetryProviderOptionsMonitor(), NullLogger<RetryProvider>.Instance),
                NullLogger<WebHookReporterOld>.Instance);

            _reporter = new Rooster.Slack.Reporting.WebHookReporter(
                new WebHookReporterOptionsMonitor(),
                client,
                new RetryProvider(new RetryProviderOptionsMonitor(), NullLogger<RetryProvider>.Instance),
                NullLogger<Rooster.Slack.Reporting.WebHookReporter>.Instance);

            var fields = new object[4]
            {
                new { title = DateTitle, value = $"`{request.ExportedLogEntry.EventDate}`" },
                new { title = ContainerNameTitle, value = $"`{request.ExportedLogEntry.ContainerName}`"},
                new { title = PortsTitle, value = $"`{request.ExportedLogEntry.InboundPort}` : `{request.ExportedLogEntry.OutboundPort}`"},
                new { title = ImageTitle, value = $"`{request.ExportedLogEntry.ImageName}`: `{request.ExportedLogEntry.ImageTag}`" }
            };

            var content =
                new
                {
                    attachments = new object[1]
                    {
                        new
                        {
                            mrkdwn_in = new object[1] { MarkdownInOption },
                            color = ColorValue,
                            pretext = $"*Service:* {request.ExportedLogEntry.ServiceName}",
                            text = $"_{message}_",
                            fields = fields
                        },
                    }
                };

            _payloadOld = content;

            _bytes = _manager.GetStream();
            JsonSerializer.SerializeAsync(_bytes, content, typeof(object));
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _bytes.Dispose();
        }

        [Benchmark]
        public Task Execute()
        {
            return _reporterOld.Send(_payloadOld, _source.Token);
        }

        [Benchmark]
        public Task Execute2()
        {
            return _reporter.Send(_bytes, _source.Token);
        }
    }

    internal class WebHookReporterOptionsMonitor : IOptionsMonitor<WebHookReporterOptions>
    {
        public WebHookReporterOptions CurrentValue => new WebHookReporterOptions { Url = "services/xxx" };

        public WebHookReporterOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<WebHookReporterOptions, string> listener)
        {
            return null;
        }
    }

    internal class RetryProviderOptionsMonitor : IOptionsMonitor<RetryProviderOptions>
    {
        public RetryProviderOptions CurrentValue => new RetryProviderOptions { Delays = new Collection<int>() };

        public RetryProviderOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<RetryProviderOptions, string> listener)
        {
            return null;
        }
    }

}
