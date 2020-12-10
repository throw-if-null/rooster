using System;
using System.Diagnostics.CodeAnalysis;

namespace Rooster.Mediator.Commands.Common.Behaviors
{
    public interface IRequestProcessingErrorBehavior
    {
        void OnError([NotNull] Exception ex);
    }
}
