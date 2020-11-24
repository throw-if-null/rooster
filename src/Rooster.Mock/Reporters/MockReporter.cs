using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.Collections.Concurrent;

namespace Rooster.Mock.Reporters
{
    public interface IMockReporter
    {
        void RegisterRequest(ShouldProcessDockerLogRequest request);
    }

    public class MockReporter : IMockReporter
    {
        private readonly ConcurrentBag<ShouldProcessDockerLogRequest> _requests;

        public MockReporter(ConcurrentBag<ShouldProcessDockerLogRequest> requests)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        public void RegisterRequest(ShouldProcessDockerLogRequest request)
        {
            _requests.Add(request);
        }
    }
}