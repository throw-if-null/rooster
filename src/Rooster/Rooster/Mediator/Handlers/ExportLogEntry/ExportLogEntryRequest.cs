using MediatR;

namespace Rooster.Mediator.Handlers.ExportLogEntry
{
    public class ExportLogEntryRequest : IRequest<ExportLogEntryResponse>
    {
        public string LogLine { get; set; }
    }
}