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

        private static void Validate(ValidateDockerRunParamsRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            request.ServiceName = CheckStringAndTrim(nameof(request.ServiceName), request.ServiceName);
            request.ContainerName = CheckStringAndTrim(nameof(request.ContainerName), request.ContainerName);
            request.ImageName = CheckStringAndTrim(nameof(request.ImageName), request.ImageName);
            request.ImageTag = CheckStringAndTrim(nameof(request.ImageTag), request.ImageTag);
            CheckInt(nameof(request.InboundPort), request.InboundPort);
            CheckInt(nameof(request.OutboundPort), request.OutboundPort);
            CheckDate(nameof(request.EventDate), request.EventDate);
        }

        static string CheckStringAndTrim(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                ThrowArgumentException(name, value == null ? Null : Empty);

            return value.Trim().ToLowerInvariant();
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
    }
}
