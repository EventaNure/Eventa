using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Application.Repositories;
using Eventa.Domain;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class TicketInCartRepository : Repository<TicketInCart>, ICartRepository
    {
        public TicketInCartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<TicketInCart?> GetTicketInCartAsync(int seatId, string userId, int eventDateTimeId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId && c.SeatId == seatId && _dbContext.Users.Any(u => u.EventDateTimeId == eventDateTimeId))
                .FirstOrDefaultAsync();
        }

        public async Task<TicketInCart?> GetTicketInCartAsync(int seatId, string userId)
        {
            return await _dbSet
                .Where(c => c.UserId == userId && c.SeatId == seatId)
                .FirstOrDefaultAsync();
        }

        public async Task<CartDataDto?> GetCartsByUserAsync(string userId)
        {
            return await _dbContext.Users
                .Where(u => u.Id == userId && u.EventDateTime != null && u.TicketsExpireAt > DateTime.UtcNow)
                .Select(u => new CartDataDto
                {
                    EventDateTimeId = u.EventDateTime!.Id,
                    EventName = u.EventDateTime.Event.Title,
                    Tickets = u.TicketsInCart.Select(c => new TicketDto
                    {
                        Price = c.Price,
                        Row = c.Seat.Row.RowNumber,
                        RowTypeName = c.Seat.Row.RowType.Name,
                        SeatId = c.Seat.Id,
                        SeatNumber = c.Seat.SeatNumber
                    }),
                    ExpireDateTime = u.TicketsExpireAt ?? new DateTime(),
                    TotalCost = u.TicketsInCart.Sum(c => c.Price)
                })
                .FirstOrDefaultAsync();
        }

        public async Task DeleteExpiredTicketsAsync()
        {
            await _dbContext.TicketsInCart
                .Where(cart => _dbContext.Users.Any(u => u.Id == cart.UserId && u.TicketsExpireAt <= DateTime.UtcNow))
                .ExecuteDeleteAsync();
        }

        public async Task DeleteTicketsForOtherEventDateTimeAsync(string userId, int eventDateTimeId)
        {
            await _dbContext.TicketsInCart
                .Where(cart => _dbContext.Users.Any(u => u.Id == cart.UserId && u.Id == userId && 
                u.EventDateTimeId != eventDateTimeId))
                .ExecuteDeleteAsync();
        }
    }
}