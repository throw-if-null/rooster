using MediatR;
using Rooster.Adapters.Kudu;
using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public record StartKuduPollerRequest : IRequest
    {
        public IKuduApiAdapter KuduAdapter { get; init; }

        public ConcurrentDictionary<string, long> Containers { get; init; }

        public double CurrentDateVarianceInMinutes { get; init; }

        public bool UseInternalPoller { get; init; }

        public double PoolingIntervalInSeconds { get; init; }
    }
}
