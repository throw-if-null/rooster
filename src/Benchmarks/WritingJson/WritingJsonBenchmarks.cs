using BenchmarkDotNet.Attributes;
using Microsoft.IO;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using Rooster.Mediator.Commands.ProcessDockerLog;
using System;
using System.Text.Json;

namespace Benchmarks.WritingJson
{
    [MemoryDiagnoser, ThreadingDiagnoser]
    public static class WritingJsonBenchmarks
    {
        private const string Message = "New container deployment.";
        private const string DateTitle = "Date";
        private const string ContainerNameTitle = "Container name";
        private const string PortsTitle = "Ports";
        private const string ImageTitle = "Image";
        private const string MarkdownInOption = "text";
        private const string ColorValue = "warning";

        private static readonly ProcessDockerLogRequest Request = new ProcessDockerLogRequest
        {
            ExtractedParams = new ExtractDockerRunParamsResponse
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
        public static string Execute()
        {
            var attachmentFields = new object[4]
            {
                new { title = DateTitle, value = $"`{Request.ExtractedParams.EventDate}`" },
                new { title = ContainerNameTitle, value = $"`{Request.ExtractedParams.ContainerName}`"},
                new { title = PortsTitle, value = $"`{Request.ExtractedParams.InboundPort}` : `{Request.ExtractedParams.OutboundPort}`"},
                new { title = ImageTitle, value = $"`{Request.ExtractedParams.ImageName}`: `{Request.ExtractedParams.ImageTag}`" }
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
                            pretext = $"*Service:* {Request.ExtractedParams.ServiceName}",
                            text = $"_{Message}_",
                            fields = attachmentFields
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
        public static Memory<byte> Execute3()
        {
            using var stream = _manager.GetStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { SkipValidation = true });

            WriteJson(writer, Request);

            return stream.ToArray();
        }

        private static void WriteJson(Utf8JsonWriter writer, ProcessDockerLogRequest request)
        {
            writer.WriteStartObject();
            writer.WriteStartArray(attachmentsKey); // start attachments

            writer.WriteStartObject();
            writer.WriteStartArray(mrkdwn_inKey);
            writer.WriteStringValue(mrkdwn_inValue);
            writer.WriteEndArray();

            writer.WriteString(colorKey, colorValue);
            writer.WriteString(pretextKey, $"*Service:* {request.ExtractedParams.ServiceName}");
            writer.WriteString(textKey, $"_{Message}_");

            writer.WriteStartArray(fieldsKey); // start fields

            writer.WriteStartObject();
            writer.WriteString(titleKey, dateTitleValue);
            writer.WriteString(valueKey, $"`{request.ExtractedParams.EventDate}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, containerNameTitleValue);
            writer.WriteString(valueKey, $"`{request.ExtractedParams.ContainerName}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, portsTitleValue);
            writer.WriteString(valueKey, $"`{request.ExtractedParams.InboundPort}` : `{request.ExtractedParams.OutboundPort}`");
            writer.WriteEndObject();

            writer.WriteStartObject();
            writer.WriteString(titleKey, imageTitleValue);
            writer.WriteString(valueKey, $"`{request.ExtractedParams.ImageName}`: `{request.ExtractedParams.ImageTag}`");
            writer.WriteEndObject();

            writer.WriteEndArray(); // end fields
            writer.WriteEndObject();

            writer.WriteEndArray(); // end attachments

            writer.WriteEndObject();

            writer.Flush();
        }
    }
}
