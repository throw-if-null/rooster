using MediatR;
using Microsoft.IO;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Slack.Reporting;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Commands.SendDockerRunParams
{
    public sealed class SlackSendDockerRunParamsCommand : SendDockerRunParamsCommand
    {
        private const string Message = "New container deployment.";
        private const string DateTitle = "Date";
        private const string ContainerNameTitle = "Container name";
        private const string PortsTitle = "Ports";
        private const string ImageTitle = "Image";
        private const string MarkdownInOption = "text";
        private const string ColorValue = "warning";

        private readonly IReporter _reporter;
        private readonly RecyclableMemoryStreamManager _streamManager;

        public SlackSendDockerRunParamsCommand(IReporter reporter, RecyclableMemoryStreamManager streamManager)
        {
            _reporter = reporter;
            _streamManager = streamManager;
        }

        protected override async Task<Unit> SendImplementation(SendDockerRunParamsRequest request, CancellationToken cancellation)
        {
            var attachmentFields = new object[4]
            {
                new { title = DateTitle, value = $"`{request.EventDate}`" },
                new { title = ContainerNameTitle, value = $"`{request.ContainerName}`"},
                new { title = PortsTitle, value = $"`{request.InboundPort}` : `{request.OutboundPort}`"},
                new { title = ImageTitle, value = $"`{request.ImageName}`: `{request.ImageTag}`" }
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
                            pretext = $"*Service:* {request.ServiceName}",
                            text = $"_{Message}_",
                            fields = attachmentFields
                        },
                    }
                };

            using var stream = _streamManager.GetStream();

            await JsonSerializer.SerializeAsync(stream, content, typeof(object), null, cancellation);

            await _reporter.Send(stream, cancellation);

            return Unit.Value;
        }
    }
}
