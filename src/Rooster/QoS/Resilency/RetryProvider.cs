using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using System;
using System.Threading.Tasks;

namespace Rooster.QoS.Resilency
{
    public interface IRetryProvider
    {
        Task<TResult> RetryOn<TException, TResult>(
            Func<TException, bool> exceptionPredicate,
            Func<TResult, bool> resultPredicate,
            Func<Task<TResult>> execute)
            where TException : Exception;
    }

    public class RetryProvider : IRetryProvider
    {
        private const string RetryAttemptLogMessage = "Retry attempt: {0}";

        private static readonly Func<int, double> CalculateJitter = delegate (int jitterMaximum)
        {
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, jitterMaximum)).TotalMilliseconds;

            return jitter;
        };

        private readonly RetryProviderOptions _options;
        private readonly ILogger _logger;

        public RetryProvider(IOptionsMonitor<RetryProviderOptions> options, ILogger<RetryProvider> logger)
        {
            _options = options?.CurrentValue ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TResult> RetryOn<TException, TResult>(
            Func<TException, bool> exceptionPredicate,
            Func<TResult, bool> resultPredicate,
            Func<Task<TResult>> execute)
            where TException : Exception
        {
            return
                Policy
                    .Handle<TException>(exceptionPredicate)
                    .OrResult(resultPredicate)
                    .WaitAndRetryAsync(
                        _options.Delays.Count,
                        i =>
                            {
                                _logger.LogInformation(RetryAttemptLogMessage, new object[1] { i });

                                var delay = _options.Delays[i - 1] + CalculateJitter(_options.JitterMaximum);

                                return TimeSpan.FromMilliseconds(delay);
                            })
                    .ExecuteAsync(execute);
        }
    }
}