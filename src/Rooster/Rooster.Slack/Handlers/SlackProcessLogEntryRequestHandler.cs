using MediatR;
using Rooster.Mediator.Requests;
using Rooster.Slack.Reporting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Handlers
{
    public class SlackProcessLogEntryRequestHandler : AsyncRequestHandler<ProcessLogEntryRequest<object>>
    {
        private readonly IReporter _reporter;

        public SlackProcessLogEntryRequestHandler(IReporter reporter)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        protected override Task Handle(ProcessLogEntryRequest<object> request, CancellationToken cancellationToken)
        {
            var message = $"Container restarted.";

            var fields = new List<object>
            {
                new { title = "Date", value = $"`{request.EventDate}`" },
                new { title = "Container name", value = $"`{request.ContainerName}`"},
                new { title = "Ports", value = $"`{request.InboundPort}` : `{request.OutboundPort}`"},
                new { title = "Image", value = $"`{request.ImageName}`" }
            };

            var content =
                new
                {
                    attachments = new object[]
                    {
                        new
                        {
                            mrkdwn_in = new[] { "text" },
                            color = "warning",
                            pretext = $"*Service:* {request.ServiceName}",
                            text = $"_{message}_",
                            fields = fields
                        },
                    }
                };

            return _reporter.Send(content, cancellationToken);
        }
    }
}