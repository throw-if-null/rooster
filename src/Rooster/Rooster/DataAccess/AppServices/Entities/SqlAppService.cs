namespace Rooster.DataAccess.AppServices.Entities
{
    public class SqlAppService : IAppService<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}