using MediatR;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using System;

namespace Rooster.Mediator.Commands.ValidateDockerRunParams
{
    public class ValidateDockerRunParamsRequest : IRequest
    {
        public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

        public string ServiceName { get; set; }

        public string ContainerName { get; set; }

        public string ImageName { get; set; }

        public string ImageTag { get; set; }

        public string InboundPort { get; set; }

        public string OutboundPort { get; set; }

        public DateTimeOffset EventDate { get; set; }

        public static implicit operator ValidateDockerRunParamsRequest(ExtractDockerRunParamsResponse response) =>
            new ExtractDockerRunParamsResponse
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