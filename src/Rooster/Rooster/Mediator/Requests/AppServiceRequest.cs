using MediatR;
using System;

namespace Rooster.Mediator.Requests
{
    public class AppServiceRequest<T> : IRequest<T>
    {
        public Uri KuduLogUri { get; set; }
    }
}