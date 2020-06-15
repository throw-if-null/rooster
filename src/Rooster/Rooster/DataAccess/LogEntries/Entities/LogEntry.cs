using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public interface ILogEntry
    {
    }

    public class LogEntry<T> : ILogEntry
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public T AppServiceId { get; set; }

        public string HostName { get; set; }

        public string ImageName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset Date { get; set; }

        public LogEntry(T appServiceId, string hostName, string imageName, string containerName, string inboundPort, string outboundPort, DateTimeOffset date)
        {
            AppServiceId = appServiceId;
            HostName = hostName;
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
                .Append($"{nameof(AppServiceId)}: {AppServiceId}, ")
                .Append($"{nameof(HostName)}:{HostName}, ")
                .Append($"{nameof(ImageName)}:{ImageName}, ")
                .Append($"{nameof(ContainerName)}:{ContainerName}, ")
                .Append($"{nameof(InboundPort)}:{InboundPort}, ")
                .Append($"{nameof(OutboundPort)}:{OutboundPort}, ")
                .Append($"{nameof(Date)}:{Date}");

            return builder.ToString();
        }
    }
}