using Rooster.DataAccess.Logbooks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rooster.DataAccess.Logbooks
{
    public class NullLogbookRepository : LogbookRepository<object>
    {
        protected override bool IsDefaultValue(object value) => false;

        protected override Task CreateImplementation(Logbook<object> logbook, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }

        protected override Task<DateTimeOffset> GetLastUpdatedDateForContainerInstanceImplementation(object kuduInstanceId, CancellationToken cancellation)
        {
            return Task.FromResult(DateTimeOffset.MinValue);
        }
    }
}
