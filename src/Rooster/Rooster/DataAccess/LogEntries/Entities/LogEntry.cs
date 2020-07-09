using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class LogEntry<T>
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public string ImageName { get; set; }

        public string WebsiteName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset Date { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder
                .Append($"{nameof(ImageName)}:{ImageName}, ")
                .Append($"{nameof(WebsiteName)}:{WebsiteName}, ")
                .Append($"{nameof(ContainerName)}:{ContainerName}, ")
                .Append($"{nameof(InboundPort)}:{InboundPort}, ")
                .Append($"{nameof(OutboundPort)}:{OutboundPort}, ")
                .Append($"{nameof(Date)}:{Date}");

            return builder.ToString();
        }
    }
}