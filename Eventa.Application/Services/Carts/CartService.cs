using Eventa.Application.Repositories;
using Eventa.Domain;
using FluentResults;

namespace Eventa.Application.Services.Carts
{
    public class CartService : ICartService
    {
        public IUnitOfWork _unitOfWork { get; set; }
        public CartService(IUnitOfWork unitOfWork) {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> BookTicket(int eventId, int seatId, string userId)
        {
            var cartRepository = _unitOfWork.GetDbSet<Cart>();
            cartRepository.Add(new Cart
            {
                 EventId = eventId,
                 SeatId = seatId,
                 UserId = userId
            });
            await _unitOfWork.CommitAsync();
            return Result.Ok();
        }
    }
}
