using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.AppInsights.Commands.HealthCheck;
using Rooster.AppInsights.DependencyInjection;
using Rooster.Hosting;
using Rooster.MongoDb.DependencyInjection;
using Rooster.MongoDb.Mediator.Commands.HealthCheck;
using Rooster.QoS.Resilency;
using Rooster.Slack.Commands.HealthCheck;
using Rooster.Slack.DependencyInjection;
using Rooster.SqlServer.DependencyInjection;
using Rooster.SqlServer.Mediator.Commands.HealthCheck;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Rooster.HealthCheck
{
    public static class HostBuilderExtensions
    {
        public static IHost ConfigureHealthCheck(this IHostBuilder builder)
        {
            var host =
                builder
                    .ConfigureWebHostDefaults(builder =>
                    {
                        builder.UseSetting(WebHostDefaults.DetailedErrorsKey, "true");

                        builder.ConfigureKestrel(x =>
                        {
                            x.ListenAnyIP(4242);
                        });

                        builder.Configure(app =>
                        {
                            app.UseRouting();

                            app.UseEndpoints(e =>
                            {
                                e.MapHealthChecks("/health", new HealthCheckOptions
                                {
                                    AllowCachingResponses = false
                                });
                            });
                        });
                    })
                    .ConfigureServices((ctx, services) =>
                    {
                        services.AddHealthChecks().AddCheck<RoosterHealthCheck>("rooster-healthcheck");

                        services.AddSingleton<IRetryProvider, RetryProvider>();

                        services.AddMediatR(new Type[]
                        {
                            typeof(AppInsightsHealthCheckRequest),
                            typeof(MongoDbHealthCheckRequest),
                            typeof(SlackHealthCheckRequest),
                            typeof(SqlServerHealthCheckRequest)
                        });

                        services.AddAppInsightsHealthCheck(ctx.Configuration);
                        services.AddMongoDbHealthCheck(ctx.Configuration);
                        services.AddSlackHealthCheck(ctx.Configuration);
                        services.AddSqlServerHealthCheck(ctx.Configuration);
                    })
                    .Build();

            return host;
        }
    }
}
