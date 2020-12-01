using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.QoS.Resilency;
using Rooster.Slack.Reporting;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Slack.Commands.HealthCheck
{
    public class SlackHealthCheckCommand : HealthCheckCommand
    {
        private static readonly Func<HttpResponseMessage, bool> TransientHttpStatusCodePredicate = delegate (HttpResponseMessage response)
        {
            if (response.StatusCode < HttpStatusCode.InternalServerError)
                return response.StatusCode == HttpStatusCode.RequestTimeout;

            return true;
        };

        private readonly WebHookReporterOptions _options;
        private readonly IRetryProvider _retryProvider;
        private readonly HttpClient _client;
        private readonly ILogger _logger;

        public SlackHealthCheckCommand(
            IOptions<WebHookReporterOptions> options,
            IRetryProvider retryProvider,
            IHttpClientFactory client,
            ILogger<SlackHealthCheckCommand> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _retryProvider = retryProvider ?? throw new ArgumentNullException(nameof(retryProvider));
            _client = client.CreateClient();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override async Task<HealthCheckResponse> Handle(HealthCheckRequest request, CancellationToken cancellationToken)
        {
            using var timeoutSource = new CancellationTokenSource(_options.TimeoutInMs);
            using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);

            try
            {
                _ = await
                    _retryProvider.RetryOn<HttpRequestException, HttpResponseMessage>(
                        x =>
                        {
                            if (!x.Data.Contains(nameof(HttpStatusCode)))
                                return false;

                            var statusCode = (HttpStatusCode)x.Data[nameof(HttpStatusCode)];

                            if (statusCode < HttpStatusCode.InternalServerError)
                                return statusCode == HttpStatusCode.RequestTimeout;

                            return false;
                        },
                        x => TransientHttpStatusCodePredicate(x),
                        () => Send(linkedSource.Token));

                return Healthy(Engines.Slack);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HealthCheck failed.", Array.Empty<object>());

                return Unhealthy(Engines.Slack, ex.ToString());
            }
        }

        private async Task<HttpResponseMessage> Send(CancellationToken cancellation)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://hooks.slack.com/" + _options.Url)
            };

            if (_options.Authorization != null)
                request.Headers.Authorization = new AuthenticationHeaderValue(_options.Authorization.Scheme, _options.Authorization.Parameter);

            _client.Timeout = TimeSpan.FromMilliseconds(_options.TimeoutInMs);

            foreach (var header in _options.Headers)
            {
                request.Headers.Add(header.Name, header.Value);
            }

            var response = await _client.SendAsync(request, cancellation);

            if (response.StatusCode >= HttpStatusCode.InternalServerError)
                throw new HttpRequestException(response.ReasonPhrase) { Data = { [nameof(HttpStatusCode)] = response.StatusCode } };

            return response;
        }
    }
}
