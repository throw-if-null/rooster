using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.SendDockerRunParams;
using Rooster.Mediator.Commands.ValidateExportedRunParams;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.Mediator.Commands.ProcessDockerLog
{
    public abstract class ProcessDockerLogCommand : IOpinionatedRequestHandler<ProcessDockerLogRequest, Unit>
    {
        protected readonly IMediator Mediator;

        protected ProcessDockerLogCommand(IMediator mediator)
        {
            Mediator = mediator;
        }

        protected abstract Task<bool> ShouldProcessDockerLog(DockerRunParams parameters, CancellationToken cancellationToken);

        public async Task<Unit> Handle(ProcessDockerLogRequest request, CancellationToken cancellationToken)
        {
            ValidateExportedRunParamsRequest validateExportedRunParamsRequest = request.ExportedLogEntry;
            ValidateExportedRunParamsResponse validateExportedRunParamsResponse =
                await Mediator.Send(validateExportedRunParamsRequest, cancellationToken);

            if (!validateExportedRunParamsResponse.IsValid)
                return Unit.Value;

            if (!await ShouldProcessDockerLog(validateExportedRunParamsRequest, cancellationToken))
                return Unit.Value;

            SendDockerRunParamsRequest sendDockerRunParamsRequest = request.ExportedLogEntry;

            return await Mediator.Send(sendDockerRunParamsRequest, cancellationToken);
        }
    }
}