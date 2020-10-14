using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.SqlServer.Connectors;
using Rooster.SqlServer.Mediator.Commands;
using Rooster.SqlServer.Mediator.Queries;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.SqlServer.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string SqlConfigPath = "DataStores:Sql";

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

        public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ConnectionFactoryOptions>(configuration.GetSection($"{SqlConfigPath}:{nameof(ConnectionFactoryOptions)}"));

            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                if (option.Tags.Any(x => x.Equals("SLACK", StringComparison.InvariantCultureIgnoreCase)))
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
                typeof(GetLatestByServiceAndContainerNamesRequest),
                typeof(CreateLogEntryRequest)
            });

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, SqlCreateLogEntryCommand>();
            services.AddTransient<IRequestHandler<
                GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>,
                SqlGetLatestByServiceAndContainerNamesQuery>();

            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, ProcessLogEntryCommand>();
            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();

            return services;
        }
    }
}