using Eventa.Application.DTOs.Orders;
using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Application.Repositories;
using Eventa.Domain;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IEnumerable<OrderListItemDto>> GetOrdersByUserAsync(string userId)
        {
            return await _dbSet
                .Where(o => o.UserId == userId && o.IsPurcharsed)
                .Select(o => new OrderListItemDto
                {
                    EventDateTimeId = o.EventDateTimeId,
                    EventName = o.EventDateTime.Event.Title,
                    OrderId = o.Id,
                    EventDateTime = o.EventDateTime.StartDateTime,
                    IsQrTokenUsed = o.IsQrTokenUsed,
                    Tickets = o.Tickets.Select(t => new TicketDto
                    {
                        Price = t.Price,
                        Row = t.Seat.Row.RowNumber,
                        RowTypeName = t.Seat.Row.RowType.Name,
                        SeatNumber = t.Seat.SeatNumber,
                        SeatId = t.SeatId
                    }),
                    TotalCost = o.Tickets.Sum(t => t.Price)
                })
                .ToListAsync();
        }

        public async Task DeleteExpireOrdersAsync()
        {
            await _dbSet
                .Where(o => !o.IsPurcharsed && o.ExpireAt < DateTime.UtcNow)
                .ExecuteDeleteAsync();
        }

        public async Task<OrderListItemDto?> GetOrderByQrTokenAsync(Guid qrToken)
        {
            return await _dbSet
                .Where(o => o.QrToken == qrToken)
                .Select(o => new OrderListItemDto
                {
                    EventDateTimeId = o.EventDateTimeId,
                    EventName = o.EventDateTime.Event.Title,
                    OrderId = o.Id,
                    IsQrTokenUsed = o.IsQrTokenUsed,
                    TotalCost = o.Tickets.Sum(t => t.Price),
                    Tickets = o.Tickets.Select(t => new TicketDto
                    {
                        Price = t.Price,
                        Row = t.Seat.Row.RowNumber,
                        RowTypeName = t.Seat.Row.RowType.Name,
                        SeatNumber = t.Seat.SeatNumber,
                        SeatId = t.SeatId
                    }),
                })
                .FirstOrDefaultAsync();
        }
    }
}
