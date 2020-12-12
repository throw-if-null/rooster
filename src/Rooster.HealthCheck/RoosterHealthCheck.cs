using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Rooster.AppInsights.Commands.HealthCheck;
using Rooster.CrossCutting;
using Rooster.Hosting;
using Rooster.Mediator.Commands.HealthCheck;
using Rooster.MongoDb.Mediator.Commands.HealthCheck;
using Rooster.Slack.Commands.HealthCheck;
using Rooster.SqlServer.Mediator.Commands.HealthCheck;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.HealthCheck
{
    public class RoosterHealthCheck : IHealthCheck
    {
        private readonly IEnumerable<string> _engines;
        private readonly IMediator _mediator;

        public RoosterHealthCheck(IConfiguration configuration, IMediator mediator)
        {
            _engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engines)}").Get<Collection<string>>();
            _mediator = mediator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();
            var isHealthy = true;

            foreach (var engine in _engines)
            {
                var response = engine.Trim().ToUpperInvariant() switch
                {
                    Engines.MongoDb => await _mediator.Send(new MongoDbHealthCheckRequest(), cancellationToken),
                    Engines.SqlServer => await _mediator.Send(new SqlServerHealthCheckRequest(), cancellationToken),
                    Engines.Slack => await _mediator.Send(new SlackHealthCheckRequest(), cancellationToken),
                    Engines.AppInsights => await _mediator.Send(new AppInsightsHealthCheckRequest(), cancellationToken),
                    Engines.Mock => new HealthCheckResponse { Name = Engines.Mock, IsHaelthy = true },
                    _ => new HealthCheckResponse { Name = "NONE", IsHaelthy = false }
                };

                isHealthy = isHealthy && response.IsHaelthy;

                data[response.Name] = new { response.IsHaelthy, response.Message };
            }

            return new HealthCheckResult(
                isHealthy ? HealthStatus.Healthy : HealthStatus.Unhealthy,
                "Rooster health status",
                data: data);
        }
    }
}
