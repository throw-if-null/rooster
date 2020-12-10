using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.Common.Behaviors;
using Rooster.Mediator.Commands.ExtractDockerRunParams;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.SendDockerRunParams
{
    public sealed record SendDockerRunParamsRequest :
        DockerRunParams,
        IRequest,
        IRequestProcessingErrorBehavior
    {
        public void OnError([NotNull] Exception ex)
        {
            return;
        }

        public static implicit operator SendDockerRunParamsRequest(ExtractDockerRunParamsResponse response) =>
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