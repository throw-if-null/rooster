using System;

namespace Rooster.DataAccess.LogEntries.Entities
{
    public class SqlLogEntry : LogEntry<int>
    {
        public SqlLogEntry(
            int appserviceId,
            string hostName,
            string imageName,
            string containerName,
            string inboundPort,
            string outboundPort,
            DateTimeOffset date)
            : base(appserviceId, hostName, imageName, containerName, inboundPort, outboundPort, date)
        {
        }

        protected override bool ValidateT(int appServiceId)
        {
            return appServiceId != default;
        }
    }
}