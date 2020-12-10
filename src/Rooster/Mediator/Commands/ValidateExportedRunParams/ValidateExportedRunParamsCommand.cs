using Rooster.Mediator.Commands.Common;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ValidateExportedRunParams
{
    public class ValidateExportedRunParamsCommand
        : IOpinionatedRequestHandler<ValidateExportedRunParamsRequest, ValidateExportedRunParamsResponse>
    {
        public Task<ValidateExportedRunParamsResponse> Handle(ValidateExportedRunParamsRequest request, CancellationToken _)
        {
            return Task.FromResult(Validate(request));
        }

        private static ValidateExportedRunParamsResponse Validate(ValidateExportedRunParamsRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            var isValid =
                CheckInt(nameof(request.InboundPort), request.InboundPort) &&
                CheckInt(nameof(request.OutboundPort), request.OutboundPort) &&
                CheckDate(nameof(request.EventDate), request.EventDate) &&
                CheckString(nameof(request.ServiceName), request.ServiceName) &&
                CheckString(nameof(request.ContainerName), request.ContainerName) &&
                CheckString(nameof(request.ImageName), request.ImageName) &&
                CheckString(nameof(request.ImageTag), request.ImageTag);

            return new() { IsValid = isValid };
        }

        static bool CheckInt(string name, string value)
        {
            return int.TryParse(value, out var integer) && integer != default;
        }

        static bool CheckDate(string name, DateTimeOffset date)
        {
            if (date != default && date != DateTimeOffset.MaxValue)
                return true;

            return false;
        }

        static bool CheckString(string name, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return true;

            return false;
        }
    }
}