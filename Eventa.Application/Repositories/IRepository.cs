namespace Eventa.Application.Repositories
{
    public interface IRepository<T> where T : class
    {
        void Add(T entity);
        void AddRange(IEnumerable<T> entities);
        void GetAllAsync();
        void GetAsync(params object[] keyValues);
        void Remove(T entity);
    }
}