using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rooster.Adapters.Kudu;
using Rooster.DependencyInjection;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Mediator.Commands.ProcessLogEntry;
using Rooster.Mock.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rooster.Test
{
    public class IntegrationTests
    {
        [Fact]
        public void ShouldThrowUnsupportedDatastoreEngineException()
        {
            var excpetion = Assert.Throws<NotSupportedEngineException>(() => TestHost.Setup("appsettings.invalid.json").Build());

            Assert.Equal($"Engine: MySql is not supported. Supported values are: {Engines.Values}.", excpetion.Message);
        }

        [Fact]
        public async Task ShouldSendOlnyNonMsiProxyDockerLog()
        {
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { (DateTimeOffset.UtcNow, new Uri("http://localhost:34"), "localhost") });

            kuduMock
                .Setup(x => x.ExtractLogsFromStream(It.IsAny<Uri>()))
                .Returns(GetValue());

            var requestsBag = new ConcurrentBag<ProcessLogEntryRequest>();
            var host =
                TestHost.Setup(
                    "appsettings.test.json",
                    (services, ctx) =>
                        {
                            services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                            services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);

                            return services;
                        })
                .Build();

            await host.StartAsync();

            Assert.Single(requestsBag);

            requestsBag.TryTake(out var request);
            Assert.DoesNotContain("msiProxy", request.ExportedLogEntry.ContainerName);

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return TestValuesBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com");

                await Task.CompletedTask;
                yield return TestValuesBuilder.BuildDockerLogLine("testContainer_msiProxy", "test:develop", "test-service", "test-service.azurewebsites.com");

                await Task.CompletedTask;
                yield return TestValuesBuilder.BuildDockerLogLine("testContainer-2_msiProxy", "test-2:develop", "test-2-service", "test-2-service.azurewebsites.com");
            }
        }

        [Fact]
        public async Task ShoudNotPickStaleMessages()
        {
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (DateTimeOffset.UtcNow.AddMinutes(-10), new Uri("http://localhost:34"), "localhost")
                });

            var requestsBag = new ConcurrentBag<ProcessLogEntryRequest>();
            var host =
                TestHost.Setup(
                    "appsettings.test.json",
                    (services, ctx) =>
                    {
                        services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);

                        return services;
                    })
                .Build();

            await host.StartAsync();

            Assert.Empty(requestsBag);
        }

        [Fact]
        public async Task ShouldReadOnlyOnce()
        {
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { (DateTimeOffset.UtcNow, new Uri("http://localhost:34"), "localhost") });

            kuduMock
                .Setup(x => x.ExtractLogsFromStream(It.IsAny<Uri>()))
                .Returns(GetValue());

            var requestsBag = new ConcurrentBag<ProcessLogEntryRequest>();
            var host =
                TestHost.Setup(
                    "appsettings.test-2.json",
                    (services, ctx) =>
                    {
                        services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);

                        return services;
                    })
                .Build();


            _ = host.StartAsync();

            await Task.Delay(4000);

            Assert.Single(requestsBag);

            await host.StopAsync();

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return TestValuesBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com");
            }
        }
    }
}