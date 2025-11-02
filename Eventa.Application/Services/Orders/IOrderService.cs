using Eventa.Application.DTOs.Orders;
using FluentResults;

namespace Eventa.Application.Services.Orders
{
    public interface IOrderService
    {
        Task<Result<OrderDto>> CreateOrderAsync(string userId, string successUrl, string cancleUrl);
        Task<Result<OrderDto>> CreateOrderAsync(string userId);
        Task<Result> DeleteExpireOrdersAsync();
        Task<Result<TimeSpan>> GetOrderDateTimeExpireAsync(int orderId);
        Task<Result<IEnumerable<OrderListItemDto>>> GetOrdersByUserAsync(string userId);
        Task<Result> HookAsync(string payload, string signature);
    }
}