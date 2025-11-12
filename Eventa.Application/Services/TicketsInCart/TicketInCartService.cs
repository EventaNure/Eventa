using AutoMapper;
using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Application.Repositories;
using Eventa.Domain;
using FluentResults;

namespace Eventa.Application.Services.TicketsInCart
{
    public class TicketInCartService : ITicketInCartService
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public IUnitOfWork _unitOfWork { get; set; }
        public TicketInCartService(IUnitOfWork unitOfWork, IUserService userService, IMapper mapper) {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Result<CartDto>> BookTicketAsync(int eventDateTimeId, int seatId, string userId)
        {
            var cartRepository = _unitOfWork.GetCartRepository();

            var seatRepository = _unitOfWork.GetSeatRepository();
            var price = await seatRepository.GetSeatPriceAsync(seatId, eventDateTimeId, userId);
            if (price == 0)
            {
                return Result.Fail(new Error("Seat for this event not exists or already taken").WithMetadata("Code", "SeatForThisEventNotExists"));
            }

            await _userService.ChangeBookingExpireTimeAsync(userId, eventDateTimeId);

            await cartRepository.DeleteTicketsForOtherEventDateTimeAsync(userId, eventDateTimeId);
            var cart = await cartRepository.GetAsync(userId, seatId);
            if (cart == null)
            {
                cart = new TicketInCart
                {
                    SeatId = seatId,
                    UserId = userId,
                    Price = price
                };
                cartRepository.Add(cart);
                await _unitOfWork.CommitAsync();
            }

            var cartDataDto = await cartRepository.GetCartsByUserAsync(userId);
            if (cartDataDto == null)
            {
                return Result.Fail(new Error("Cart not found").WithMetadata("Code", "CartNotFound"));
            }
            var cartDto = _mapper.Map<CartDto>(cartDataDto);
            cartDto.ExpireAt = cartDataDto.ExpireDateTime - DateTime.UtcNow;
            return Result.Ok(cartDto);
        }

        public async Task<Result<CartDto>> GetCartsByUserAsync(string userId)
        {
            var cartRepository = _unitOfWork.GetCartRepository();
            var cartDataDto = await cartRepository.GetCartsByUserAsync(userId);

            if (cartDataDto == null)
            {
                return Result.Fail(new Error("Cart not exists").WithMetadata("Code", "CartNotExists"));
            }
            var cartDto = _mapper.Map<CartDto>(cartDataDto);
            cartDto.ExpireAt = cartDataDto.ExpireDateTime - DateTime.UtcNow;

            return Result.Ok(cartDto);
        }

        public async Task<Result> DeleteExpiredTicketsAsync()
        {
            var cartRepository = _unitOfWork.GetCartRepository();
            await cartRepository.DeleteExpiredTicketsAsync();
            return Result.Ok();
        }

        public async Task<Result<CartDto>> DeleteTicketAsync(int seatId, string userId)
        {
            var cartRepository = _unitOfWork.GetCartRepository();
            var cart = await cartRepository.GetTicketInCartAsync(seatId, userId);
            if (cart == null)
            {
                return Result.Fail(new Error("This ticket not exists in your cart").WithMetadata("Code", "TicketInCartNotExists"));
            }
            cartRepository.Remove(cart);
            await _unitOfWork.CommitAsync();
            var cartDataDto = await cartRepository.GetCartsByUserAsync(userId);
            if (cartDataDto == null)
            {
                return Result.Fail(new Error("Cart not found").WithMetadata("Code", "CartNotFound"));
            }
            var cartDto = _mapper.Map<CartDto>(cartDataDto);
            cartDto.ExpireAt = cartDataDto.ExpireDateTime - DateTime.UtcNow;
            return Result.Ok(cartDto);
        }
    }
}
