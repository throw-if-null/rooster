using System;

namespace Rooster.Adapters.Kudu
{
    public class KuduLog
    {
        public DateTimeOffset LastUpdated { get; set; }

        public Uri Href { get; set; }

        public string MachineName { get; set; }
    }
}
