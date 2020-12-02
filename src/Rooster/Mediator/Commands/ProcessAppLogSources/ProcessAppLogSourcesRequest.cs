using MediatR;
using Rooster.Adapters.Kudu;

namespace Rooster.Mediator.Commands.ProcessAppLogSources
{
    public record ProcessAppLogSourcesRequest : IRequest
    {
        public IKuduApiAdapter Kudu { get; init; }

        public double CurrentDateVarianceInMinutes { get; init; }
    }
}