using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
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
        public async Task ShouldThrowUnsupportedDatastoreEngineException()
        {

            var excpetion = await Assert.ThrowsAsync<NotSupportedEngineException>(() => TestRunner.Run("appsettings.invalid.json", (_, __) => { }));

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

            await
                TestRunner.Run(
                    "appsettings.test.json",
                    (ctx, services) =>
                        {
                            services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                            services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                        });

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

            await
                TestRunner.Run(
                    "appsettings.test.json",
                    (ctx, services) =>
                    {
                        services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                    });

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

            await
                TestRunner.Run(
                    "appsettings.test-2.json",
                    (ctx, services) =>
                    {
                        services.AddSingleton<ConcurrentBag<ProcessLogEntryRequest>>(requestsBag);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                    });

            await Task.Delay(4000);

            Assert.Single(requestsBag);

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return TestValuesBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com");
            }
        }
    }
}