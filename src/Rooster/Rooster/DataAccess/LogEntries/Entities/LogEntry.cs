using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class LogEntry<T>
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public string ServiceName { get; set; }

        public string ContainerName { get; set; }

        public string ImageName { get; set; }

        public string ImageTag { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder
                .Append($"{nameof(ServiceName)}:{ServiceName}, ")
                .Append($"{nameof(ContainerName)}:{ContainerName}, ")
                .Append($"{nameof(ImageName)}:{ImageName}, ")
                .Append($"{nameof(ImageTag)}:{ImageTag}, ")
                .Append($"{nameof(InboundPort)}:{InboundPort}, ")
                .Append($"{nameof(OutboundPort)}:{OutboundPort}, ")
                .Append($"{nameof(EventDate)}:{EventDate}");

            return builder.ToString();
        }
    }
}