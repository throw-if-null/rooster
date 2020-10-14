using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using Rooster.Mediator.Commands.CreateLogEntry;
using Rooster.Mediator.Commands.ExportLogEntry;
using Rooster.Mediator.Commands.ProcessDockerLogs;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using Rooster.MongoDb.Connectors.Clients;
using Rooster.MongoDb.Connectors.Colections;
using Rooster.MongoDb.Connectors.Databases;
using Rooster.MongoDb.Mediator.Commands;
using Rooster.MongoDb.Mediator.Queries;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.MongoDb.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private const string MongoDbPath = "DataStores:MongoDb";

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

        private static string BuildMongoCollectionFactoryPath<T>()
        {
            return $"{MongoDbPath}:{nameof(CollectionFactoryOptions)}:{typeof(T).Name}";
        }

        public static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.ConfigureClientFactoryOptions(configuration);
            services.ConfigureDatabaseFactoryOptions(configuration);
            services.ConfigureLogEntryCollectionFactoryOptions(configuration);

            services.AddSingleton<IClientFactory, ClientFactory>();
            services.AddSingleton<IDatabaseFactory, DatabaseFactory>();
            services.AddSingleton<ILogEntryCollectionFactory, LogEntryCollectionFactory>();

            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                if (option.Tags.Any(x => x.Equals("MONGODB", StringComparison.InvariantCultureIgnoreCase)))
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

            services.AddTransient<IRequestHandler<CreateLogEntryRequest, Unit>, MongoDbCreateLogEntryCommand>();
            services.AddTransient<
                IRequestHandler<GetLatestByServiceAndContainerNamesRequest, DateTimeOffset>,
                MongoDbGetLatestByServiceAndContainerNamesQuery>();

            services.AddTransient<IRequestHandler<ExportLogEntryRequest, ExportLogEntryResponse>, ExportLogEntryCommand>();
            services.AddTransient<IRequestHandler<ProcessDockerLogsRequest, ProcessDockerLogsResponse>, ProcessDockerLogsCommand>();
            services.AddTransient<IRequestHandler<ProcessLogEntryRequest, Unit>, ProcessLogEntryCommand>();

            return services;

        }

        private static IServiceCollection ConfigureClientFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection($"{MongoDbPath}:{nameof(ClientFactoryOptions)}");

            return services.Configure<ClientFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureDatabaseFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection($"{MongoDbPath}:{nameof(DatabaseFactoryOptions)}");

            return services.Configure<DatabaseFactoryOptions>(section);
        }

        private static IServiceCollection ConfigureLogEntryCollectionFactoryOptions(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var section = configuration.GetSection(BuildMongoCollectionFactoryPath<LogEntryCollectionFactoryOptions>());

            return services.Configure<LogEntryCollectionFactoryOptions>(section);
        }
    }
}