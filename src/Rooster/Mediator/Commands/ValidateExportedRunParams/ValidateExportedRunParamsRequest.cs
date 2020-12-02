using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ExtractDockerRunParams;

namespace Rooster.Mediator.Commands.ValidateExportedRunParams
{
    public sealed record ValidateExportedRunParamsRequest :
        DockerRunParams,
        IRequest<ValidateExportedRunParamsResponse>
    {
        public static implicit operator ValidateExportedRunParamsRequest(ExtractDockerRunParamsResponse response) =>
            new()
            {
                Created = response.Created,
                ServiceName = response.ServiceName,
                ContainerName = response.ContainerName,
                ImageName = response.ImageName,
                ImageTag = response.ImageTag,
                InboundPort = response.InboundPort,
                OutboundPort = response.OutboundPort,
                EventDate = response.EventDate
            };
    }
}
