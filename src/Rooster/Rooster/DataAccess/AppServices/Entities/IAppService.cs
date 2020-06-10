namespace Rooster.DataAccess.AppServices.Entities
{
    public interface IAppService
    {
    }

    public interface IAppService<T> : IAppService
    {
        public T Id { get; set; }

        public string Name { get; set; }
    }
}