using MediatR;
using Rooster.Adapters.Kudu;
using System.Collections.Concurrent;

namespace Rooster.Mediator.Commands.ProcessKuduLogs
{
    public class ProcessKuduLogsRequest : IRequest
    {
        public IKuduApiAdapter KuduAdapter { get; set; }

        public ConcurrentDictionary<string, long> Containers { get; set; }

        public double CurrentDateVarianceInMinutes { get; set; }

        public bool UseInternalPoller { get; internal set; }

        public double PoolingIntervalInSeconds { get; internal set; }
    }
}
