using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessLogSource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessAppLogSources
{
    public class ProcessAppLogSourcesCommand : IOpinionatedRequestHandler<ProcessAppLogSourcesRequest, Unit>
    {
        private const string LogIsOldMessage = "Log: {0} is old. Last updated: {1}. Machine: {2}";

        private readonly ILogger _logger;
        private readonly IMediator _mediator;

        public ProcessAppLogSourcesCommand(ILogger<ProcessAppLogSourcesCommand> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Unit> Handle(ProcessAppLogSourcesRequest request, CancellationToken cancellationToken)
        {
            var kuduLogs = await request.Kudu.GetDockerLogs(cancellationToken);

            var logsPerMachine =
                kuduLogs
                    .Where(x => x.LastUpdated.Date == DateTimeOffset.UtcNow.Date)
                    .GroupBy(x => x.MachineName)
                    .Select(x => x.OrderByDescending(x => x.LastUpdated).First())
                    .ToList();

            var tasks = new List<Task>(logsPerMachine.Count);

            foreach ((DateTimeOffset lastUpdated, Uri logUri, string machineName) in logsPerMachine)
            {
                var lastUpdatedSecondsEx = lastUpdated.ToUniversalTime().AddSeconds(request.CurrentDateVarianceInSeconds).ToUnixTimeSeconds();
                var currentSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if (lastUpdatedSecondsEx < currentSeconds)
                {
                    _logger.LogDebug(LogIsOldMessage, logUri, lastUpdated, machineName);

                    continue;
                }

                tasks.Add(_mediator.Send(new ProcessLogSourceRequest
                {
                    Kudu = request.Kudu,
                    LastUpdated = lastUpdated,
                    LogUri = logUri,
                    MachineName = machineName
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);

            return Unit.Value;
        }
    }
}
