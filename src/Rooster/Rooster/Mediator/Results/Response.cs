using MediatR;

namespace Rooster.Mediator.Results
{
    public abstract class Response
    {
        public bool Failed { get; set; }

        public string Message { get; set; }
    }
}
