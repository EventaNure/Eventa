using Eventa.Application.DTOs.Seats;
using Eventa.Application.DTOs.Sections;

namespace Eventa.Application.Repositories
{
    public interface ISeatRepository
    {
        Task<GetFreeSeatsResultDto?> GetFreeSeatsAsync(int eventId);
    }
}