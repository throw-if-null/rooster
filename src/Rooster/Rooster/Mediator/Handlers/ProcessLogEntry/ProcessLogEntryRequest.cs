using MediatR;
using Rooster.Mediator.Handlers.ExportLogEntry;

namespace Rooster.Mediator.Handlers.ProcessLogEntry
{
    public class ProcessLogEntryRequest : IRequest
    {
        public ExportLogEntryResponse ExportedLogEntry { get; set; }
    }
}