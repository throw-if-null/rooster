using MediatR;
using System;

namespace Rooster.Mediator.Requests
{
    public class LogbookRequest<T> : IRequest<DateTimeOffset>
    {
        public T ContainerInstanceId { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public string MachineName { get; set; }
    }
}