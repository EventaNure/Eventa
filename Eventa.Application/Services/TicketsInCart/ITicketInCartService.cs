using Eventa.Application.DTOs.TicketInCarts;
using FluentResults;

namespace Eventa.Application.Services.TicketsInCart
{
    public interface ITicketInCartService
    {
        Task<Result<CartDto>> BookTicketAsync(int eventDateTimeId, int seatId, string userId);
        Task<Result> DeleteExpiredTicketsAsync();
        Task<Result<CartDto>> DeleteTicketAsync(int seatId, string userId);
        Task<Result<CartDto>> GetCartsByUserAsync(string userId);
    }
}