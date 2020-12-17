using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.ProcessAppLogSources
{
    public record ProcessAppLogSourcesRequest :
        IRequest,
        IRequestProcessingErrorBehavior
    {
        private readonly ILogger _logger;

        public ProcessAppLogSourcesRequest(ILogger logger)
        {
            _logger = logger;
        }

        public IKuduApiAdapter Kudu { get; init; }

        public int CurrentDateVarianceInSeconds { get; init; }

        public void OnError([NotNull] Exception ex)
        {
            _logger.LogWarning(
                ex,
                "{Command} failed. Data: {KuduUrl}",
                nameof(ProcessAppLogSourcesCommand),
                Kudu.BaseUrl);
        }
    }
}