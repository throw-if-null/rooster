using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.StartKuduPoller
{
    public record StartKuduPollerRequest :
        IRequest,
        IRequestProcessingErrorBehavior
    {
        private readonly ILogger _logger;

        public StartKuduPollerRequest(ILogger logger)
        {
            _logger = logger;
        }

        public IKuduApiAdapter KuduAdapter { get; init; }

        public int CurrentDateVarianceInSeconds { get; init; }

        public bool UseInternalPoller { get; init; }

        public double PoolingIntervalInSeconds { get; init; }

        public void OnError([NotNull] Exception ex)
        {
            _logger.LogWarning(ex, "{Command} failed.", nameof(StartKuduPollerCommand));
        }
    }
}
