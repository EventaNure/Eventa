namespace Eventa.Application.Repositories
{
    public interface IUnitOfWork
    {
        IRepository<T> GetDbSet<T>() where T : class;
    }
}