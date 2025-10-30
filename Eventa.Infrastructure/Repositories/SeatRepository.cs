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

        public async Task<GetFreeSeatsResultDto?> GetFreeSeatsAsync(int eventId)
        {
            var freeSeats = await _dbContext.Events
                .Where(e => e.Id == eventId)
                .Select(e => new GetFreeSeatsResultDto
                {
                    PlaceId = e.PlaceId,
                    RowTypes = e.Place.RowTypes.Select(rt => new RowTypeDto
                    {
                        Name = rt.Name,
                        Rows = rt.Rows.Select(r => new RowDto
                        {
                            Id = r.Id,
                            RowNumber = r.RowNumber,
                            Seats = r.Seats
                                .Where(s => !s.Carts.Any(c => c.EventId == eventId))
                                .Select(s => new SeatDto
                                {
                                    Id = s.Id,
                                    SeatNumber = s.SeatNumber,
                                    Price = s.PriceMultiplier,
                                }).ToArray()
                        }).ToArray()
                    }).ToList()
                }
           ).FirstOrDefaultAsync();
           return freeSeats;
        }
    }
}
