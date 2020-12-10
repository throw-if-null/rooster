using Rooster.Mediator.Commands.SendDockerRunParams;
using System;
using System.Collections.Concurrent;

namespace Rooster.Mock.Reporters
{
    public interface IMockReporter
    {
        void RegisterRequest(SendDockerRunParamsRequest request);
    }

    public class MockReporter : IMockReporter
    {
        private readonly ConcurrentDictionary<string, int> _requests;

        public MockReporter(ConcurrentDictionary<string, int> requests)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        public void RegisterRequest(SendDockerRunParamsRequest request)
        {
            if (_requests.ContainsKey(request.ContainerName))
                _requests[request.ContainerName] += 1;
            else
                _requests[request.ContainerName] = 1;
        }
    }
}