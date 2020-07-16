using MediatR;
using Rooster.Mediator.Results;

namespace Rooster.Mediator.Requests
{
    public class ProcessLogEntryRequest<T> : IRequest
    {
        public ExportLogEntryResponse ExportedLogEntry { get; set; }
    }
}