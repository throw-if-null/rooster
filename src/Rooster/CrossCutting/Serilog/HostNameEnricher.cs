using Serilog.Core;
using Serilog.Events;

namespace Rooster.CrossCutting.Serilog
{
    public sealed class HostNameEnricher : ILogEventEnricher
    {
        private const string HostName = "HostName";

        private readonly string _hostName;

        public HostNameEnricher(string hostName)
        {
            _hostName = hostName;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(HostName, _hostName));
        }
    }
}
