
using Eventa.Application.DTOs.Orders;

namespace Eventa.Application.Repositories
{
    public interface IOrderRepository
    {
        Task DeleteExpireOrdersAsync();
        Task<IEnumerable<OrderListItemDto>> GetOrdersByUserAsync(string userId);
    }
}