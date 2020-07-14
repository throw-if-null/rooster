using MediatR;

namespace Rooster.Mediator.Requests
{
    public class ExportLogEntryRequest<T> : IRequest<ProcessLogEntryRequest<T>>
    {
        public string LogLine { get; set; }
    }
}