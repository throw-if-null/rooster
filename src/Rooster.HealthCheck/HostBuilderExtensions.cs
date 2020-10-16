using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rooster.QoS.HealthChecks;

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
                    })
                    .Build();

            return host;
        }
    }
}
