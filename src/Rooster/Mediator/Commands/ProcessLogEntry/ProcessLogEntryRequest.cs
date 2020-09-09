using MediatR;
using Rooster.Mediator.Commands.ExportLogEntry;

namespace Rooster.Mediator.Commands.ProcessLogEntry
{
    public class ProcessLogEntryRequest : IRequest
    {
        public ExportLogEntryResponse ExportedLogEntry { get; set; }
    }
}