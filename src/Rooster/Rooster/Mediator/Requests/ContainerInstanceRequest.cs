using MediatR;

namespace Rooster.Mediator.Requests
{
    public class ContainerInstanceRequest<T> : IRequest<T>
    {
        public string MachineName { get; set; }

        public T AppServiceId { get; set; }
    }
}