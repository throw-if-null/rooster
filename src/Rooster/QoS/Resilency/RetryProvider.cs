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
        private static readonly Func<double> GetJitter = delegate ()
        {
            var jitter = TimeSpan.FromMilliseconds(new Random().Next(0, 100)).TotalMilliseconds;

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
                                _logger.LogInformation($"Retry attempt: {i}", Array.Empty<object>());

                                return TimeSpan.FromMilliseconds(_options.Delays[i - 1] + GetJitter());
                            })
                    .ExecuteAsync(execute);
        }
    }
}