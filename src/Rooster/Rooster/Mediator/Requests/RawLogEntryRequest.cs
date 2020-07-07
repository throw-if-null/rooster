using MediatR;

namespace Rooster.Mediator.Requests
{
    public class RawLogEntryRequest<T> : IRequest<LogEntryRequest<T>>
    {
        public string LogLine { get; set; }
    }
}