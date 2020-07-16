using MediatR;
using Rooster.Mediator.Results;

namespace Rooster.Mediator.Requests
{
    public class ExportLogEntryRequest : IRequest<ExportLogEntryResponse>
    {
        public string LogLine { get; set; }
    }
}