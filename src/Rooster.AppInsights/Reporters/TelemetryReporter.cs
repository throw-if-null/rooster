using Microsoft.ApplicationInsights;
using System;
using System.Collections.Generic;

namespace Rooster.AppInsights.Reporters
{
    public interface ITelemetryReporter
    {
        void Report(IDictionary<string, string> properties);
    }

    public class TelemetryReporter : ITelemetryReporter
    {
        private readonly TelemetryClient _client;

        public TelemetryReporter(TelemetryClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public void Report(IDictionary<string, string> properties)
        {
            _client.TrackEvent("ContainerRestart", properties);

            _client.Flush();
        }
    }
}