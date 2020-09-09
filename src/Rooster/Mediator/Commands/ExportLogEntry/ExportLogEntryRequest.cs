using MediatR;

namespace Rooster.Mediator.Commands.ExportLogEntry
{
    public class ExportLogEntryRequest : IRequest<ExportLogEntryResponse>
    {
        public string LogLine { get; set; }
    }
}