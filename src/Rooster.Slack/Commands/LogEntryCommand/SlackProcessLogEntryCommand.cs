using MediatR;
using Microsoft.IO;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Slack.Reporting;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Commands.LogEntryCommand
{
    public class SlackProcessLogEntryCommand : AsyncRequestHandler<ShouldProcessDockerLogRequest>
    {
        private const string message = "New container deployment.";
        private const string DateTitle = "Date";
        private const string ContainerNameTitle = "Container name";
        private const string PortsTitle = "Ports";
        private const string ImageTitle = "Image";
        private const string MarkdownInOption = "text";
        private const string ColorValue = "warning";

        private readonly IReporter _reporter;
        private readonly RecyclableMemoryStreamManager _streamManager;

        public SlackProcessLogEntryCommand(IReporter reporter, RecyclableMemoryStreamManager streamManager)
        {
            _reporter = reporter;
            _streamManager = streamManager;
        }

        protected override async Task Handle(ShouldProcessDockerLogRequest request, CancellationToken cancellationToken)
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

            using var stream = _streamManager.GetStream();

            await JsonSerializer.SerializeAsync(stream, content, typeof(object), null, cancellationToken);

            await _reporter.Send(stream, cancellationToken);
        }
    }
}