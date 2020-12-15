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
using System;
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
            _engines = configuration.GetSection($"{nameof(AppHostOptions)}:{nameof(Engine)}").Get<Collection<string>>();
            _mediator = mediator;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();
            var isHealthy = true;

            foreach (Engine engine in Engine.ToList(_engines))
            {
                var response = new HealthCheckResponse { Name = "NONE", IsHaelthy = false };

                if (engine.Equals(Engine.MongoDb))
                {
                    response = await _mediator.Send(new MongoDbHealthCheckRequest(), cancellationToken);
                }
                else if (engine.Equals(Engine.SqlServer))
                {
                    response = await _mediator.Send(new SqlServerHealthCheckRequest(), cancellationToken);
                }
                else if (engine.Equals(Engine.Slack))
                {
                    response = await _mediator.Send(new SlackHealthCheckRequest(), cancellationToken);
                }
                else if (engine.Equals(Engine.AppInsights))
                {
                    response = await _mediator.Send(new AppInsightsHealthCheckRequest(), cancellationToken);
                }
                else if (engine.Equals(Engine.Mock))
                {
                    response = new HealthCheckResponse { Name = Engine.Mock.Name, IsHaelthy = true };
                }

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
