using MediatR;
using Rooster.Mediator.Commands.Common;
using Rooster.Mediator.Commands.ProcessDockerLog;
using Rooster.Mediator.Queries.GetLatestByServiceAndContainerNames;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.SqlServer.Mediator.Commands.ProcessDockerLog
{
    public class SqlProcessDockerLogCommand : ProcessDockerLogCommand
    {
        public SqlProcessDockerLogCommand(IMediator mediator) : base(mediator)
        {
        }

        protected override async Task<bool> ShouldProcessDockerLog(
            DockerRunParams parameters,
            CancellationToken cancellationToken)
        {
            var latestLogEntry =
                await
                    Mediator.Send(
                        new GetLatestByServiceAndContainerNamesRequest
                        {
                            ContainerName = parameters.ContainerName,
                            ServiceName = parameters.ServiceName
                        },
                        cancellationToken);

            return parameters.EventDate > latestLogEntry;
        }
    }
}
