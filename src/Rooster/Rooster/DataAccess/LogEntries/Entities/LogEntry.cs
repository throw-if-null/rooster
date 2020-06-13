using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public interface ILogEntry
    {
    }

    public abstract class LogEntry<T> : ILogEntry
    {
        public T Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public T AppServiceId { get; set; }

        public string HostName { get; set; }

        public string ImageName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset Date { get; set; }

        protected LogEntry(T appserviceId, string hostName, string imageName, string containerName, string inboundPort, string outboundPort, DateTimeOffset date)
        {
            if (!ValidateT(appserviceId))
                throw new ArgumentException($"{nameof(AppServiceId)} {appserviceId} must have a valid value.");

            if (string.IsNullOrWhiteSpace(hostName))
                throw new ArgumentNullException(nameof(hostName));

            if(string.IsNullOrWhiteSpace(imageName))
                throw new ArgumentNullException(nameof(imageName));

            if(string.IsNullOrWhiteSpace(containerName))
                throw new ArgumentNullException(nameof(containerName));

            if (!int.TryParse(inboundPort, out var _))
                throw new ArgumentException($"{nameof(InboundPort)} {inboundPort} must have a valid value.");

            if (!int.TryParse(outboundPort, out var _))
                throw new ArgumentException($"{nameof(OutboundPort)} {outboundPort} must have a valid value.");

            if (date == default || date == DateTimeOffset.MaxValue)
                throw new ArgumentException($"{nameof(date)} {date} must have a valid value.");

            AppServiceId = appserviceId;
            HostName = hostName;
            ImageName = imageName;
            ContainerName = containerName;
            InboundPort = inboundPort;
            OutboundPort = outboundPort;
            Date = date;
        }

        protected abstract bool ValidateT(T appServiceId);

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