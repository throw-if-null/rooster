using MediatR;
using Rooster.CrossCutting.Exceptions;
using Rooster.Mediator.Commands.Common.Behaviors;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.InitKuduPollers
{
    public record InitKuduPollersRequest : IRequest, IRequestProcessingErrorBehavior
    {
        public void OnError([NotNull] Exception ex)
        {
            if (ex is PollingCanceledException)
                return;

            throw new OopsieDaisyException(ex);
        }
    }
}
