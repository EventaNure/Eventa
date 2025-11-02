using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface ICartRepository : IRepository<TicketInCart>
    {
        Task DeleteExpiredTicketsAsync();
        Task<TicketInCart?> GetTicketInCartAsync(int seatId, string userId, int eventDateTimeId);
        Task<CartDataDto?> GetCartsByUserAsync(string userId);
        Task DeleteTicketsForOtherEventDateTimeAsync(string userId, int eventDateTimeId);
        Task<TicketInCart?> GetTicketInCartAsync(int seatId, string userId);
    }
}