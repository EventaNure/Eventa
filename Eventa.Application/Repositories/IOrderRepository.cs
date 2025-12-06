

using Eventa.Application.DTOs.Orders;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task DeleteExpireOrdersAsync();
        Task<OrderListItemDto?> GetOrderByQrTokenAsync(Guid qrToken);
        Task<IEnumerable<OrderListItemDto>> GetOrdersByUserAsync(string userId);
        Task<Order?> GetOrderWithCommentAsync(int orderId);
    }
}