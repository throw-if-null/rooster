using MediatR;
using Rooster.DataAccess.Logbooks.Entities;

namespace Rooster.Mediator.Notifications
{
    public class LogbookNotification<T> : Logbook<T>, INotification
    {
        public LogbookNotification(Logbook<T> logbook)
        {
            Id = logbook.Id;
            ContainerInstanceId = logbook.ContainerInstanceId;
            Created = logbook.Created;
            Href = logbook.Href;
            LastUpdated = logbook.LastUpdated;
            MachineName = logbook.MachineName;
            Path = logbook.Path;
            Size = logbook.Size;
        }
    }
}