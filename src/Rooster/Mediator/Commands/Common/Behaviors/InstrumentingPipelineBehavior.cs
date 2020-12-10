using MediatR;
using Microsoft.Extensions.Logging;
using Rooster.CrossCutting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.Common.Behaviors
{
    public class InstrumentingPipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly ILogger _logger;

        public InstrumentingPipelineBehavior(ILogger<InstrumentingPipelineBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var requestType = request.GetType().Name;
            var stopwatch = ValueStopwatch.StartNew();

            try
            {
                _logger.LogDebug("{RequestType} started.", requestType);

                var response = await next();

                _logger.LogDebug("{RequestType} - finished. {Elapsed}", requestType, stopwatch.GetElapsedTime().Milliseconds.ToString());

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{RequestType} failed.", requestType);

                var behavior = (IRequestProcessingErrorBehavior)request;
                behavior.OnError(ex);

                return default;
            }
        }
    }
}
