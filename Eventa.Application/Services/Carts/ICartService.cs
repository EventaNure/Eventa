using FluentResults;

namespace Eventa.Application.Services.Carts
{
    public interface ICartService
    {
        Task<Result> BookTicket(int eventId, int seatId, string userId);
    }
}