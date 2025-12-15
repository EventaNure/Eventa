using Eventa.Application.DTOs.Seats;
using Eventa.Application.DTOs.Sections;
using Eventa.Application.Repositories;
using Eventa.Domain;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Repositories
{
    public class SeatRepository : Repository<Seat>, ISeatRepository
    {
        public SeatRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<GetFreeSeatsResultDto?> GetFreeSeatsAsync(int eventDateTimeId, string? userId)
        {
            var usersWithEvent = await _dbContext.Users
                .Where(u => u.EventDateTimeId == eventDateTimeId)
                .Select(u => u.Id)
                .ToListAsync();

            var eventDateTime = await _dbContext.EventDateTimes
                .Include(e => e.Event)
                    .ThenInclude(ev => ev.Place)
                        .ThenInclude(p => p.RowTypes)
                            .ThenInclude(rt => rt.Rows)
                                .ThenInclude(r => r.Seats)
                                    .ThenInclude(s => s.TicketsInCart)
                .Include(e => e.Event.Place.RowTypes)
                    .ThenInclude(rt => rt.Rows)
                        .ThenInclude(r => r.Seats)
                            .ThenInclude(s => s.TicketsInOrder)
                                .ThenInclude(tio => tio.Order)
                .FirstOrDefaultAsync(e => e.Id == eventDateTimeId);

            if (eventDateTime == null)
                return null;

            var freeSeats = new GetFreeSeatsResultDto
            {
                PlaceId = eventDateTime.Event.PlaceId,
                RowTypes = eventDateTime.Event.Place.RowTypes.Select(rt => new RowTypeDto
                {
                    Name = rt.Name,
                    Rows = rt.Rows.Select(r => new RowDto
                    {
                        Id = r.Id,
                        RowNumber = r.RowNumber,
                        Seats = r.Seats
                            .Where(s =>
                                !s.TicketsInCart.Any(c => usersWithEvent.Contains(c.UserId)) &&
                                !s.TicketsInOrder.Any(tInO =>
                                    tInO.Order.EventDateTimeId == eventDateTimeId &&
                                    (tInO.Order.IsPurcharsed ||
                                     (!tInO.Order.IsPurcharsed && tInO.Order.UserId != userId))))
                            .Select(s => new SeatDto
                            {
                                Id = s.Id,
                                SeatNumber = s.SeatNumber,
                                Price = s.PriceMultiplier * eventDateTime.Event.Price
                            })
                            .ToArray()
                    }).ToArray()
                }).ToList()
            };

            return freeSeats;
        }

        public async Task<double> GetSeatPriceAsync(int seatId, int eventDateTimeId, string userId)
        {
            return await _dbContext.Seats
                .Where(s => s.Id == seatId
                    && s.Row.RowType.Place.Events.Any(e => e.EventDateTimes.Any(edt => edt.Id == eventDateTimeId))
                    && !s.TicketsInCart.Any(tInC => _dbContext.Users.Any(u => u.TicketsExpireAt > DateTime.UtcNow && u.EventDateTimeId == eventDateTimeId))
                    && !s.TicketsInOrder.Any(tInO =>
                    tInO.Order.EventDateTimeId == eventDateTimeId && (tInO.Order.IsPurcharsed || (!tInO.Order.IsPurcharsed && tInO.Order.UserId != userId))))
                .Select(s => 
                    s.PriceMultiplier * s.Row.RowType.Place.Events
                    .First(e => e.EventDateTimes.Any(edt => edt.Id == eventDateTimeId)).Price
                )
                .FirstOrDefaultAsync();
        }
    }
}
