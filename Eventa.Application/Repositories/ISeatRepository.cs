using Eventa.Application.DTOs.Seats;
using Eventa.Domain;

namespace Eventa.Application.Repositories
{
    public interface ISeatRepository : IRepository<Seat>
    {
        Task<GetFreeSeatsResultDto?> GetFreeSeatsAsync(int eventDateTimeId, string? userId);
        Task<double> GetSeatPriceAsync(int seatId, int eventId, string userId);
    }
}