using Eventa.Application.Repositories;
using Eventa.Domain;

namespace Eventa.Infrastructure.Repositories
{
    public class CartRepository : Repository<Cart>, ICartRepositroy
    {
        public CartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> Exists()
        {
            return true;
        }
    }
}
