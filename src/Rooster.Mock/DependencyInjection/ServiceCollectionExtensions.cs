using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.CrossCutting.Serilog;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mock.Commands;
using Rooster.Mock.Reporters;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.Mock.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<string, string, AuthenticationHeaderValue> BuildBasicAuthHeader =
            delegate (string user, string password)
            {
                if (string.IsNullOrWhiteSpace(user))
                    throw new ArgumentNullException(nameof(user));

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                var base64 = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{user}:{password}"));

                return new AuthenticationHeaderValue("Basic", base64);
            };

        public static IServiceCollection AddMock(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new HostNameEnricher(nameof(MockHost)));
            services.AddSingleton(new ConcurrentBag<ProcessLogEntryRequest>());

            services.AddTransient<IMockReporter, MockReporter>();

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                if (option.Tags.Any(x => x.Equals("MOCK", StringComparison.InvariantCultureIgnoreCase)))
                    services
                        .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                        {
                            x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                            x.BaseAddress = option.BaseUri;
                        });
            }

            services.AddMediatR(new[]
            {
                typeof(ProcessLogEntryRequest),
                typeof(ExportLogEntryRequest),
                typeof(ProcessDockerLogsRequest),
                typeof(CreateLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, MockProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            services.AddHostedService<MockHost>();

            return services;
        }
    }
}