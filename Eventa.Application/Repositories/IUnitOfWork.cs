namespace Eventa.Application.Repositories
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
        ICartRepository GetCartRepository();
        IRepository<T> GetDbSet<T>() where T : class;
        IEventRepository GetEventRepository();
        IOrderRepository GetOrderRepository();
        ISeatRepository GetSeatRepository();
        ITagRepository GetTagRepository();
    }
}