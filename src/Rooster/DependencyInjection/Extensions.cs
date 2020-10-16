using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rooster.Adapters.Kudu;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Rooster.DependencyInjection
{
    public static class Extensions
    {
        public static IServiceCollection AddKuduClient(
            this IServiceCollection services,
            IConfiguration configuration,
            [NotNull] string engine)
        {
            var options = configuration.GetSection($"Adapters:{nameof(KuduAdapterOptions)}").Get<Collection<KuduAdapterOptions>>();

            foreach (var option in options ?? Enumerable.Empty<KuduAdapterOptions>())
            {
                if (!option.Tags.Any() ||
                    option.Tags.Any(tag => tag.Equals(engine, StringComparison.InvariantCultureIgnoreCase)))
                {
                    services
                        .AddHttpClient<IKuduApiAdapter, KuduApiAdapter>($"Kudu-{Guid.NewGuid():N}", x =>
                        {
                            x.DefaultRequestHeaders.Authorization = BuildBasicAuthHeader(option.User, option.Password);
                            x.BaseAddress = option.BaseUri;
                        });
                }
            }

            return services;
        }

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
    }
}
