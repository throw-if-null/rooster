using MediatR;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using System;

namespace Rooster.Mediator.Commands.ValidateDockerRunParams
{
    public record ValidateDockerRunParamsRequest : IRequest
    {
        public DateTimeOffset Created { get; init; } = DateTimeOffset.UtcNow;

        public string ServiceName { get; init; }

        public string ContainerName { get; init; }

        public string ImageName { get; init; }

        public string ImageTag { get; init; }

        public string InboundPort { get; init; }

        public string OutboundPort { get; init; }

        public DateTimeOffset EventDate { get; init; }

        public static implicit operator ValidateDockerRunParamsRequest(ExtractDockerRunParamsResponse response) =>
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