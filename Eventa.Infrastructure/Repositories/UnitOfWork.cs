using Eventa.Application.Repositories;

namespace Eventa.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly Dictionary<Type, object> _repositories = [];

        public UnitOfWork(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        public IRepository<T> GetDbSet<T>() where T : class
        {
            var type = typeof(T);
            if (!_repositories.ContainsKey(type))
            {
                _repositories.Add(type, new Repository<T>(_dbContext));
            }
            return (IRepository<T>)_repositories[type];
        }

        public IEventRepository GetEventRepository() => new EventRepository(_dbContext);

        public ITagRepository GetTagRepository() => new TagRepository(_dbContext);

        public ISeatRepository GetSeatRepository() => new SeatRepository(_dbContext);

        public ICartRepository GetCartRepository() => new TicketInCartRepository(_dbContext);

        public IOrderRepository GetOrderRepository() => new OrderRepository(_dbContext);

        public async Task CommitAsync() => await _dbContext.SaveChangesAsync();

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
