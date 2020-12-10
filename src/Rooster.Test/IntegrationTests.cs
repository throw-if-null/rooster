using Microsoft.Extensions.DependencyInjection;
using Moq;
using Rooster.Adapters.Kudu;
using Rooster.DependencyInjection.Exceptions;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Commands.SendDockerRunParams;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Rooster.Test
{
    public class MsiProxyDockerLogs
    {
        [Fact]
        public async Task ShouldNotRead()
        {
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { (DateTimeOffset.UtcNow, new Uri("http://localhost:34"), "localhost") });

            kuduMock
                .Setup(x => x.ExtractLogsFromStream(It.IsAny<Uri>()))
                .Returns(GetValue());

            var requests = new ConcurrentDictionary<string, int>();

            var host = TestRunner.Run(
                "appsettings.test.json",
                (ctx, services) =>
                {
                    services.AddSingleton<ConcurrentDictionary<string, int>>(requests);

                    services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                });

            await host.StartAsync();

            Assert.Single(requests);
            Assert.True(requests.TryGetValue("testContainer", out var request));

            await host.StopAsync();
            host.Dispose();

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return DockerRunParamsBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com", inboundPort: 42.ToString(), outboundPort: 42.ToString());

                await Task.CompletedTask;
                yield return DockerRunParamsBuilder.BuildDockerLogLine("testContainer_msiProxy", "test:develop", "test-service", "test-service.azurewebsites.com", inboundPort: 42.ToString(), outboundPort: 42.ToString());

                await Task.CompletedTask;
                yield return DockerRunParamsBuilder.BuildDockerLogLine("testContainer-2_msiProxy", "test-2:develop", "test-2-service", "test-2-service.azurewebsites.com", inboundPort: 42.ToString(), outboundPort: 42.ToString());
            }
        }
    }

    public class StaleMessages
    {
        [Fact]
        public async Task ShoudNotRead()
        {
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    (DateTimeOffset.UtcNow.AddMinutes(-10), new Uri("http://localhost:34"), "localhost")
                });

            var requests = new ConcurrentDictionary<string, int>();

            var host =
                TestRunner.Run(
                    "appsettings.test.json",
                    (ctx, services) =>
                    {
                        services.AddSingleton<ConcurrentDictionary<string, int>>(requests);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                    });

            await host.StopAsync();
            Assert.Empty(requests);

            await host.StopAsync();
            host.Dispose();
        }
    }

    public class PollingIntervalSmallerThenCurrentDateTimeTollerance
    {
        [Fact]
        public async Task ShouldReadSameLogMultipleTimes()
        {
            long currentTicks = DateTimeOffset.UtcNow.Ticks;
            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { (new DateTimeOffset(currentTicks, DateTimeOffset.UtcNow.Offset), new Uri("http://localhost:34"), "localhost") });

            kuduMock
                .Setup(x => x.ExtractLogsFromStream(It.IsAny<Uri>()))
                .Returns(GetValue());

            var requests = new ConcurrentDictionary<string, int>();

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(8));
            var host = TestRunner.Run(
                "appsettings.test-2.json",
                (ctx, services) =>
                {
                    services.AddSingleton<ConcurrentDictionary<string, int>>(requests);

                    services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                });

            var canceledTask = Assert.ThrowsAsync<TaskCanceledException>(() => host.StartAsync(source.Token));

            await canceledTask;

            Assert.Single(requests);

            requests.TryGetValue("testContainer", out var count);
            Assert.True(count > 1);

            host.Dispose();

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return DockerRunParamsBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com", inboundPort: 42.ToString(), outboundPort: 42.ToString());
            }
        }
    }

    public class PollingIntervalLargerThenCurrentDateTimeTollerance
    {
        [Fact]
        public async Task ShouldReadOnlyOnce()
        {
            long currentTicks = DateTimeOffset.UtcNow.Ticks;

            var kuduMock = new Mock<IKuduApiAdapter>();
            kuduMock
                .Setup(x => x.GetDockerLogs(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { (new DateTimeOffset(currentTicks, DateTimeOffset.UtcNow.Offset), new Uri("http://localhost:34"), "localhost") });

            kuduMock
                .Setup(x => x.ExtractLogsFromStream(It.IsAny<Uri>()))
                .Returns(GetValue());

            var requests = new ConcurrentDictionary<string, int>();

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(7));
            var host = TestRunner.Run(
                    "appsettings.test-3.json",
                    (ctx, services) =>
                    {
                        services.AddSingleton<ConcurrentDictionary<string, int>>(requests);

                        services.AddTransient<IKuduApiAdapter>(x => kuduMock.Object);
                    });

            var canceledTask = Assert.ThrowsAsync<TaskCanceledException>(() => host.StartAsync(source.Token));

            await canceledTask;

            Assert.Single(requests);

            requests.TryGetValue("testContainer", out var count);
            Assert.Equal(1, count);

            host.Dispose();

            static async IAsyncEnumerable<string> GetValue()
            {
                await Task.CompletedTask;
                yield return DockerRunParamsBuilder.BuildDockerLogLine("testContainer", "test:develop", "test-service", "test-service.azurewebsites.com", inboundPort: 42.ToString(), outboundPort: 42.ToString());
            }
        }
    }
}