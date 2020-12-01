using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ValidateDockerRunParams
{
    public abstract class ValidateDockerRunParamsCommand : IRequestHandler<ValidateDockerRunParamsRequest>
    {
        private const string Null = "NULL";
        private const string Empty = "EMPTY";

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };

        protected abstract Task<Unit> CreateImplementation(ValidateDockerRunParamsRequest request, CancellationToken cancellation);

        public Task<Unit> Handle(ValidateDockerRunParamsRequest request, CancellationToken cancellationToken)
        {
            Validate(request);

            return CreateImplementation(request, cancellationToken);
        }

        private static ValidateDockerRunParamsRequest Validate(ValidateDockerRunParamsRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            CheckInt(nameof(request.InboundPort), request.InboundPort);
            CheckInt(nameof(request.OutboundPort), request.OutboundPort);
            CheckDate(nameof(request.EventDate), request.EventDate);
            CheckString(nameof(request.ServiceName), request.ServiceName);
            CheckString(nameof(request.ContainerName), request.ContainerName);
            CheckString(nameof(request.ImageName), request.ImageName);
            CheckString(nameof(request.ImageTag), request.ImageTag);

            return request with
            {
                ServiceName = request.ServiceName.Trim().ToLowerInvariant(),
                ContainerName = request.ContainerName.Trim().ToLowerInvariant(),
                ImageName = request.ImageName.Trim().ToLowerInvariant(),
                ImageTag = request.ImageTag.Trim().ToLowerInvariant()
            };
        }

        static void CheckInt(string name, string value)
        {
            if (int.TryParse(value, out var integer) && integer != default)
                return;

            ThrowArgumentException(name, value);
        }

        static void CheckDate(string name, DateTimeOffset date)
        {
            if (date != default && date != DateTimeOffset.MaxValue)
                return;

            ThrowArgumentException(name, date.ToString());
        }

        static void CheckString(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return;

            ThrowArgumentException(name, value == null ? Null : Empty);
        }
    }
}
