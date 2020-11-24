using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.ProcessAppLogSource
{
    public class ProcessAppLogSourceResponse
    {
        public ConcurrentDictionary<string, long> Containers { get; set; }
    }
}
