using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rooster.Mediator.Notifications
{
    public class LogEntryNotification<T> : INotification
    {
        public string LogLine { get; set; }

        public T ContainerInstanceId { get; set; }
    }
}
