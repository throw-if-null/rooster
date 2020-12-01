using Rooster.CrossCutting.Docker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Rooster.Test
{
    public class LogExtractorTests
    {
        [Fact]
        public void ShouldExtractCorrectParameters()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test:1.0",
                "test-service",
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal("testContainer", dockerRunParameters.ContainerName.ToString());
            Assert.Equal("test", dockerRunParameters.ImageName.ToString());
            Assert.Equal("1.0", dockerRunParameters.ImageTag.ToString());
            Assert.Equal("test-service", dockerRunParameters.ServiceName.ToString());
            Assert.Equal(42, dockerRunParameters.InboundPort);
            Assert.Equal(4242, dockerRunParameters.OutboundPort);
            Assert.Equal(date, dockerRunParameters.Date);
        }

        [Fact]
        public void ShouldHaveEmptyContainerOrServiceNameWhenNotProvided()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                null,
                "test:1.0",
                null,
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Empty(dockerRunParameters.ContainerName.ToString());
            Assert.Empty(dockerRunParameters.ServiceName.ToString());
        }

        [Fact]
        public void ShouldHaveEmptyImageAndTagWhenTheyAreNotProvided()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                null,
                "test-services",
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Empty(dockerRunParameters.ImageName.ToString());
            Assert.Empty(dockerRunParameters.ImageTag.ToString());
        }

        [Fact]
        public void ShouldResolveImageNameWhenTagIsNotSet()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test-img",
                "test-services",
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal("test-img", dockerRunParameters.ImageName.ToString());
            Assert.Empty(dockerRunParameters.ImageTag.ToString());
        }

        [Fact]
        public void ShouldResolveImageTagWhenImageNameIsNotSet()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                ":1.0",
                "test-services",
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Empty(dockerRunParameters.ImageName.ToString());
            Assert.Equal("1.0", dockerRunParameters.ImageTag.ToString());
        }

        [Fact]
        public void ShouldResolveImageNameAndTagWhenTagContainsDoubleColumn()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test:1.0:invalidTag",
                "test-services",
                "test",
                date,
                42.ToString(),
                4242.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal("test", dockerRunParameters.ImageName.ToString());
            Assert.Equal("1.0:invalidTag", dockerRunParameters.ImageTag.ToString());
        }

        [Fact]
        public void ShouldReturnNegativeOneWhenPortsDontHaveNumericValues()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test:1.0:invalidTag",
                "test-services",
                "test",
                date,
                null,
                "not-a-number").AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal(-1, dockerRunParameters.InboundPort);
            Assert.Equal(-1, dockerRunParameters.OutboundPort);
        }

        [Fact]
        public void ShouldResolveInboundPortOnly()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test:1.0:invalidTag",
                "test-services",
                "test",
                date,
                42.ToString(),
                string.Empty).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal(42, dockerRunParameters.InboundPort);
            Assert.Equal(-1, dockerRunParameters.OutboundPort);
        }

        [Fact]
        public void ShouldResolveOutboundPortOnly()
        {
            var date = DateTimeOffset.UtcNow.AddHours(-1);
            var dockerRunLine = DockerRunParamsBuilder.BuildDockerLogLine(
                "testContainer",
                "test:1.0:invalidTag",
                "test-services",
                "test",
                date,
                string.Empty,
                42.ToString()).AsSpan();

            var dockerRunParameters = LogExtractor.Extract(dockerRunLine);

            Assert.Equal(-1, dockerRunParameters.InboundPort);
            Assert.Equal(42, dockerRunParameters.OutboundPort);
        }
    }
}
