using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.ProcessDockerLogs
{
    public class ProcessDockerLogsResponse
    {
        public ConcurrentDictionary<string, long> Containers { get; set; }
    }
}
