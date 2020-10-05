using Rooster.Mediator.Commands.ProcessLogEntry;
using System;
using System.Collections.Concurrent;

namespace Rooster.Mock.Reporters
{
    public interface IMockReporter
    {
        void RegisterRequest(ProcessLogEntryRequest request);
    }

    public class MockReporter : IMockReporter
    {
        private readonly ConcurrentBag<ProcessLogEntryRequest> _requests;

        public MockReporter(ConcurrentBag<ProcessLogEntryRequest> requests)
        {
            _requests = requests ?? throw new ArgumentNullException(nameof(requests));
        }

        public void RegisterRequest(ProcessLogEntryRequest request)
        {
            _requests.Add(request);
        }
    }
}