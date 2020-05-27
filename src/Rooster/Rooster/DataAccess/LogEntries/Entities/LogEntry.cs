using System;
using System.Text;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }

        public DateTimeOffset Created { get; set; }

        public string AppServiceName { get; set; }

        public string HostName { get; set; }

        public string ImageName { get; set; }

        public string ContainerName { get; set; }

        public string InboundPort { get; set; }

        public string OutbouondPort { get; set; }

        public DateTimeOffset Date { get; set; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder
                .Append($"{nameof(AppServiceName)}: {AppServiceName}, ")
                .Append($"{nameof(HostName)}:{HostName}, ")
                .Append($"{nameof(ImageName)}:{ImageName}, ")
                .Append($"{nameof(ContainerName)}:{ContainerName}, ")
                .Append($"{nameof(InboundPort)}:{InboundPort}, ")
                .Append($"{nameof(OutbouondPort)}:{OutbouondPort}, ")
                .Append($"{nameof(Date)}:{Date}");

            return builder.ToString();
        }
    }
}