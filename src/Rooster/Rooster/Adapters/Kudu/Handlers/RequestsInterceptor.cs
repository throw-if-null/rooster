using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Adapters.Kudu.Handlers
{
    public sealed class RequestsInterceptor : DelegatingHandler
    {
        private const string Slash = "/";
        private const string WildCard = "*";

        private readonly char[] _splitter = Slash.ToCharArray();

        private readonly Collection<HttpInterceptorOptions> _options;
        private readonly IHostEnvironment _environment;
        private readonly ILogger _logger;

        public RequestsInterceptor(IOptions<Collection<HttpInterceptorOptions>> options, IHostEnvironment environment, ILogger<RequestsInterceptor> logger)
        {
            _options = options?.Value ?? new Collection<HttpInterceptorOptions>();
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_environment.IsProduction())
                _logger.LogWarning("Request intercepting for Kudu API is configured for production use.", null);

            _ = request ?? throw new ArgumentNullException(nameof(request));

            var options = _options.Where(x => x.MethodName.Equals(request.Method.Method, StringComparison.InvariantCultureIgnoreCase));

            foreach (var option in options)
            {
                var path = option.Path.StartsWith(Slash, StringComparison.InvariantCultureIgnoreCase) ? option.Path : $"/{option.Path}";

                if (path.Contains(WildCard))
                {
                    var matchSegments = path.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);
                    var segments = request.RequestUri.PathAndQuery.Split(_splitter, StringSplitOptions.RemoveEmptyEntries);

                    var isMatch = matchSegments.Length.Equals(segments.Length);

                    if (!isMatch)
                        continue;

                    for (var i = 0; i < matchSegments.Length; i++)
                    {
                        var match = matchSegments[i];
                        var segment = segments[i];

                        isMatch = match.Equals(segment, StringComparison.InvariantCultureIgnoreCase) || match == WildCard;

                        if (!isMatch)
                            break;
                    }

                    if (isMatch)
                        return option.TryCreateResponse();
                }

                if (path.Equals(request.RequestUri.PathAndQuery, StringComparison.InvariantCultureIgnoreCase))
                    return option.TryCreateResponse();
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}