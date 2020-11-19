using BenchmarkDotNet.Attributes;
using Microsoft.IO;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

namespace Benchmarks.WritingJson
{
    [MemoryDiagnoser, ThreadingDiagnoser]
    public class WritingJsonBenchmarks
    {
        private const string message = "New container deployment.";
        private const string DateTitle = "Date";
        private const string ContainerNameTitle = "Container name";
        private const string PortsTitle = "Ports";
        private const string ImageTitle = "Image";
        private const string MarkdownInOption = "text";
        private const string ColorValue = "warning";

        private static ProcessLogEntryRequest request = new ProcessLogEntryRequest
        {
            ExportedLogEntry = new ExportLogEntryResponse
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

        [Benchmark]
        public string Execute()
        {
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

            return JsonSerializer.Serialize(content);
        }

        private static readonly RecyclableMemoryStreamManager _manager = new RecyclableMemoryStreamManager();

        private static readonly JsonEncodedText attachmentsKey = JsonEncodedText.Encode("attachments");
        private static readonly JsonEncodedText mrkdwn_inKey = JsonEncodedText.Encode("mrkdwn_in");
        private static readonly JsonEncodedText mrkdwn_inValue = JsonEncodedText.Encode(MarkdownInOption);
        private static readonly JsonEncodedText colorKey = JsonEncodedText.Encode("color");
        private static readonly JsonEncodedText colorValue = JsonEncodedText.Encode(ColorValue);
        private static readonly JsonEncodedText pretextKey = JsonEncodedText.Encode("pretext");
        private static readonly JsonEncodedText textKey = JsonEncodedText.Encode("text");
        private static readonly JsonEncodedText fieldsKey = JsonEncodedText.Encode("fields");
        private static readonly JsonEncodedText titleKey = JsonEncodedText.Encode("title");
        private static readonly JsonEncodedText dateTitleValue = JsonEncodedText.Encode(DateTitle);
        private static readonly JsonEncodedText valueKey = JsonEncodedText.Encode("value");
        private static readonly JsonEncodedText containerNameTitleValue = JsonEncodedText.Encode(ContainerNameTitle);
        private static readonly JsonEncodedText portsTitleValue = JsonEncodedText.Encode(PortsTitle);
        private static readonly JsonEncodedText imageTitleValue = JsonEncodedText.Encode(ImageTitle);

        [Benchmark]
        public Memory<byte> Execute3()
        {
            using var stream = _manager.GetStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { SkipValidation = true });

            WriteJson(writer, request);

            return stream.ToArray();
        }

        private static void WriteJson(Utf8JsonWriter writer, ProcessLogEntryRequest request)
        {
            writer.WriteStartObject();
            writer.WriteStartArray(attachmentsKey); // start attachments

            writer.WriteStartObject();
            writer.WriteStartArray(mrkdwn_inKey);
            writer.WriteStringValue(mrkdwn_inValue);
            writer.WriteEndArray();

            writer.WriteString(colorKey, colorValue);
            writer.WriteString(pretextKey, $"*Service:* {request.ExportedLogEntry.ServiceName}");
            writer.WriteString(textKey, $"_{message}_");

            writer.WriteStartArray(fieldsKey); // start fields

            writer.WriteStartObject();
            writer.WriteString(titleKey, dateTitleValue);
            writer.WriteString(valueKey, $"`{request.ExportedLogEntry.EventDate}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, containerNameTitleValue);
            writer.WriteString(valueKey, $"`{request.ExportedLogEntry.ContainerName}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, portsTitleValue);
            writer.WriteString(valueKey, $"`{request.ExportedLogEntry.InboundPort}` : `{request.ExportedLogEntry.OutboundPort}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, imageTitleValue);
            writer.WriteString(valueKey, $"`{request.ExportedLogEntry.ImageName}`: `{request.ExportedLogEntry.ImageTag}`");
            writer.WriteEndObject();

            writer.WriteEndArray(); // end fields
            writer.WriteEndObject();

            writer.WriteEndArray(); // end attachments

            writer.WriteEndObject();

            writer.Flush();
        }
    }
}
