using MediatR;
using Rooster.Adapters.Kudu;
using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.ProcessAppLogSource
{
    public class ProcessAppLogSourceRequest : IRequest<ProcessAppLogSourceResponse>
    {
        public IKuduApiAdapter Kudu { get; set; }

        public double CurrentDateVarianceInMinutes { get; set; }

        public ConcurrentDictionary<string, long> Containers { get; set; }
    }
}