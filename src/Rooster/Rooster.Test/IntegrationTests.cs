using Rooster.DependencyInjection.Exceptions;
using Xunit;

namespace Rooster.Test
{
    public class IntegrationTests
    {
        [Fact]
        public void ShouldThrowUnsupportedDatastoreEngineException()
        {
            var excpetion = Assert.Throws<NotSupportedDataStoreException>(() => TestHost.Setup("appsettings.invalid.json").Build());

            Assert.Equal("Database: MySql is not supported. Supported values are: MongoDb and SqlServer.", excpetion.Message);
        }
    }
}