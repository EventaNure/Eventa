using Eventa.Application.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _dbContext;

        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public void Add(T entity) => _dbSet.Add(entity);

        public void Remove(T entity) => _dbSet.Remove(entity);

        public void GetAsync(params object[] keyValues) => _dbSet.FindAsync(keyValues);

        public void GetAllAsync() => _dbSet.ToListAsync();
    }
}
