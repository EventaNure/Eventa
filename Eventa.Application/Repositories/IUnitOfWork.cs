namespace Eventa.Application.Repositories
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
        IRepository<T> GetDbSet<T>() where T : class;
        IEventRepository GetEventRepository();
        ISeatRepository GetSectionRepository();
        ITagRepository GetTagRepository();
    }
}