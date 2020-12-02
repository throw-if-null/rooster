using MediatR;
using Rooster.Adapters.Kudu;
using System;

namespace Rooster.Mediator.Commands.ProcessLogSource
{
    public sealed record ProcessLogSourceRequest : IRequest
    {
        public DateTimeOffset LastUpdated { get; init; }

        public Uri LogUri { get; init; }

        public string MachineName { get; init; }

        public IKuduApiAdapter Kudu { get; init; }
    }
}