using MediatR;
using Rooster.Adapters.Kudu;
using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.ProcessAppLogSource
{
    public record ProcessAppLogSourceRequest : IRequest<ProcessAppLogSourceResponse>
    {
        public IKuduApiAdapter Kudu { get; init; }

        public double CurrentDateVarianceInMinutes { get; init; }

        public ConcurrentDictionary<string, long> Containers { get; init; }
    }
}