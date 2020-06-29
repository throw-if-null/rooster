using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class LogEntry<T>
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public T LogbookId { get; set; }

        public string ImageName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset Date { get; set; }

        public LogEntry(T appServiceId, string imageName, string containerName, string inboundPort, string outboundPort, DateTimeOffset date)
        {
            LogbookId = appServiceId;
            ImageName = imageName;
            ContainerName = containerName;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            Date = date;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder
                .Append($"{nameof(LogbookId)}: {LogbookId}, ")
                .Append($"{nameof(ImageName)}:{ImageName}, ")
                .Append($"{nameof(ContainerName)}:{ContainerName}, ")
                .Append($"{nameof(InboundPort)}:{InboundPort}, ")
                .Append($"{nameof(OutboundPort)}:{OutboundPort}, ")
                .Append($"{nameof(Date)}:{Date}");

            return builder.ToString();
        }
    }
}