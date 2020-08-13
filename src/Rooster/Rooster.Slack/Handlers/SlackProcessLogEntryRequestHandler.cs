using MediatR;
using Rooster.Mediator.Requests;
using Rooster.Slack.Reporting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Handlers
{
    public class SlackProcessLogEntryRequestHandler : AsyncRequestHandler<ProcessLogEntryRequest>
    {
        private readonly IReporter _reporter;

        public SlackProcessLogEntryRequestHandler(IReporter reporter)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        protected override Task Handle(ProcessLogEntryRequest request, CancellationToken cancellationToken)
        {
            var message = $"Container restarted.";

            var fields = new object[4]
            {
                new { title = "Date", value = $"`{request.ExportedLogEntry.EventDate}`" },
                new { title = "Container name", value = $"`{request.ExportedLogEntry.ContainerName}`"},
                new { title = "Ports", value = $"`{request.ExportedLogEntry.InboundPort}` : `{request.ExportedLogEntry.OutboundPort}`"},
                new { title = "Image", value = $"`{request.ExportedLogEntry.ImageName}`" }
            };

            var content =
                new
                {
                    attachments = new object[1]
                    {
                        new
                        {
                            mrkdwn_in = new object[1] { "text" },
                            color = "warning",
                            pretext = $"*Service:* {request.ExportedLogEntry.ServiceName}",
                            text = $"_{message}_",
                            fields = fields
                        },
                    }
                };

            return _reporter.Send(content, cancellationToken);
        }
    }
}