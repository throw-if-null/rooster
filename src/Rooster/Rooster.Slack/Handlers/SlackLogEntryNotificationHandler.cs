using Rooster.CrossCutting;
using Rooster.DataAccess.LogEntries;
using Rooster.Mediator.Handlers;
using Rooster.Mediator.Notifications;
using Rooster.Slack.Reporting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Handlers
{
    public class SlackLogEntryNotificationHandler : LogEntryNotificationHandler<object>
    {
        private readonly IReporter _reporter;

        public SlackLogEntryNotificationHandler(
            ILogEntryRepository<object> logEntryRepository,
            ILogExtractor extractor,
            IReporter reporter)
            : base(logEntryRepository, extractor)
        {
            _reporter = reporter ?? throw new ArgumentNullException(nameof(reporter));
        }

        public override Task Handle(LogEntryNotification<object> notification, CancellationToken cancellationToken)
        {
            var (inboundPort, outboundPort) = Extractor.ExtractPorts(notification.LogLine);

            var websiteName = Extractor.ExtractWebsiteName(notification.LogLine);
            var imageName = Extractor.ExtractImageName(notification.LogLine);
            var containerName = Extractor.ExtractContainerName(notification.LogLine);
            var logDate = Extractor.ExtractDate(notification.LogLine);

            var message = $"Container restarted.";

            var fields = new List<object>
            {
                new { title = "Date", value = $"`{logDate}`" },
                new { title = "Container name", value = $"`{containerName}`"},
                new { title = "Ports", value = $"`{inboundPort}` : `{outboundPort}`"},
                new { title = "Image", value = $"`{imageName}`" }
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
                            pretext = $"*Service:* {websiteName}",
                            text = $"_{message}_",
                            fields = fields
                        },
                    }
                };

            return _reporter.Send(content, cancellationToken);
        }

        private static double UtcNowToUnixTimestamp(DateTimeOffset date)
        {
            TimeSpan difference = date.ToUniversalTime() - DateTimeOffset.UnixEpoch;

            return Math.Floor(difference.TotalSeconds);
        }

    }
}