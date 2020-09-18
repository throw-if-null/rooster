using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.CreateLogEntry
{
    public abstract class CreateLogEntryCommand : IRequestHandler<CreateLogEntryRequest>
    {
        private const string Null = "NULL";
        private const string Empty = "EMPTY";

        private static readonly Action<string, string> ThrowArgumentException = delegate (string name, string value)
        {
            throw new ArgumentException($"{name} has invalid value: [{value}].");
        };

        protected abstract Task<Unit> CreateImplementation(CreateLogEntryRequest request, CancellationToken cancellation);

        public Task<Unit> Handle(CreateLogEntryRequest request, CancellationToken cancellationToken)
        {
            Validate(request);

            return CreateImplementation(request, cancellationToken);
        }

        private static void Validate(CreateLogEntryRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            CheckString(nameof(request.ServiceName), request.ServiceName);
            CheckString(nameof(request.ContainerName), request.ContainerName);
            CheckString(nameof(request.ImageName), request.ImageName);
            CheckString(nameof(request.ImageTag), request.ImageTag);
            CheckInt(nameof(request.InboundPort), request.InboundPort);
            CheckInt(nameof(request.OutboundPort), request.OutboundPort);
            CheckDate(nameof(request.EventDate), request.EventDate);

            static void CheckString(string name, string value)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    name = name.Trim().ToLowerInvariant();

                    return;
                }

                ThrowArgumentException(name, value == null ? Null : Empty);
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
}
