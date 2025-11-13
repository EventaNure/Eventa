using AutoMapper;
using Eventa.Application.DTOs.Orders;
using Eventa.Application.DTOs.TicketInCarts;
using Eventa.Application.Repositories;
using Eventa.Domain;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace Eventa.Application.Services.Orders
{
    public class OrderService : IOrderService
    {
        private const string orderIdMetadataName = "OrderId";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IPaymentService _paymentService;
        private readonly IUserService _userService;
        private readonly ILogger<Order> _logger;
        private readonly IMapper _mapper;

        public OrderService(IUnitOfWork unitOfWork, IPaymentService paymentService, IUserService userService, ILogger<Order> logger, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _paymentService = paymentService;
            _userService = userService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<Result<OrderDto>> CreateOrderAsync(string userId, string successUrl, string cancleUrl)
        {
            var addOrderResult = await AddOrderAsync(userId);
            if (!addOrderResult.IsSuccess)
            {
                return Result.Fail(addOrderResult.Errors);
            }
            var orderDto = addOrderResult.Value;
            var items = orderDto.Tickets.Select(t => new ItemWithPriceDto
            {
                Price = (int)(Math.Ceiling(t.Price) * 100),
                Name = $"{orderDto.EventName}. {t.RowTypeName}. Row: {t.Row}. Seat: {t.SeatNumber}"
            });
            orderDto.SessionId = await _paymentService.CreateCheckoutSessionAsync(orderDto.OrderId, items, successUrl, cancleUrl, orderIdMetadataName);

            return orderDto;
        }

        public async Task<Result<OrderDto>> CreateOrderAsync(string userId)
        {
            var addOrderResult = await AddOrderAsync(userId);
            if (!addOrderResult.IsSuccess)
            {
                return Result.Fail(addOrderResult.Errors);
            }
            var orderDto = addOrderResult.Value;

            orderDto.SessionId = await _paymentService.CreatePaymentIntentAsync(orderDto.OrderId, orderIdMetadataName, orderDto.TotalPrice);

            return orderDto;
        }

        public async Task<Result<OrderDto>> AddOrderAsync(string userId)
        {
            var orderDbSet = _unitOfWork.GetDbSet<Order>();
            var cartRepository = _unitOfWork.GetCartRepository();
            var cartDataDto = await cartRepository.GetCartsByUserAsync(userId);
            if (cartDataDto == null || !cartDataDto.Tickets.Any())
            {
                return Result.Fail(new Error("Cart is empty").WithMetadata("Code", "CartEmpty"));
            }
            var cartDto = _mapper.Map<CartDto>(cartDataDto);
            cartDto.ExpireAt = cartDataDto.ExpireDateTime - DateTime.UtcNow;
            var getBookingDateTimeExpireResult = await _userService.GetBookingDateTimeExpireAsync(userId);
            if (!getBookingDateTimeExpireResult.IsSuccess)
            {
                return Result.Fail(getBookingDateTimeExpireResult.Errors);
            }
            var order = new Order
            {
                EventDateTimeId = cartDataDto.EventDateTimeId,
                UserId = userId,
                Tickets = cartDataDto.Tickets.Select(t => new TicketInOrder
                {
                    Price = t.Price,
                    SeatId = t.SeatId
                }).ToList(),
                QrToken = Guid.NewGuid(),
                ExpireAt = getBookingDateTimeExpireResult.Value
            };
            orderDbSet.Add(order);
            await _unitOfWork.CommitAsync();

            return new OrderDto
            {
                OrderId = order.Id,
                EventDateTimeId = order.EventDateTimeId,
                EventName = cartDto.EventName,
                ExpireAt = order.ExpireAt - DateTime.UtcNow,
                Tickets = cartDto.Tickets,
                TotalPrice = cartDto.TotalCost
            };
        }

        public async Task<Result> HookAsync(string payload, string signature)
        {
            if (!_paymentService.IsPaymentIntentSuccess(payload, signature))
            {
                _logger.LogError("Payment not success");
                return Result.Fail(new Error("Payment failed").WithMetadata("Code", "PaymentFailed"));
            }

            var orderId = _paymentService.GetMetadataFromPayment(payload, signature, orderIdMetadataName);
            if (orderId == null)
            {
                _logger.LogError("Not get metadata");
                return Result.Fail(new Error("Payment failed").WithMetadata("Code", "PaymentFailed"));
            }

            var orderRepository = _unitOfWork.GetDbSet<Order>();
            var parseResult = int.TryParse(orderId, out int parseOrderId);
            var order = await orderRepository.GetAsync(parseOrderId);
            if (order == null)
            {
                _logger.LogError($"Order not found for id {orderId}");
                return Result.Fail(new Error("Payment failed").WithMetadata("Code", "PaymentFailed"));
            }

            order.IsPurcharsed = true;
            var cartRepository = _unitOfWork.GetCartRepository();
            await cartRepository.DeleteTicketsForOtherEventDateTimeAsync(order.UserId, 0);
            await _userService.DeleteInformationAboutCartAsync(order.UserId);
            await _unitOfWork.CommitAsync();
            _logger.LogError("IsPurcharsed change");
            return Result.Ok();
        }

        public async Task<Result<GenerateQrCodeResultDto>> GenerateQRTokenAsync(int orderId, string userId)
        {
            var orderRepository = _unitOfWork.GetDbSet<Order>();

            var order = await orderRepository.GetAsync(orderId);

            if (order == null)
            {
                return Result.Fail(new Error("Order not found").WithMetadata("Code", "OrderNotFound"));
            }

            return Result.Ok(new GenerateQrCodeResultDto
            {
                QrToken = order.IsQrTokenUsed ? null : order.QrToken,
                IsQrTokenUsed = order.IsQrTokenUsed,
                QrCodeUsingDateTime = order.QrCodeUsingDateTime
            });
        }

        public async Task<Result> DeleteExpireOrdersAsync()
        {
            var orderRepository = _unitOfWork.GetOrderRepository();
            await orderRepository.DeleteExpireOrdersAsync();
            return Result.Ok();
        }

        public async Task<Result<TimeSpan>> GetOrderDateTimeExpireAsync(int orderId)
        {
            var orderDbSet = _unitOfWork.GetDbSet<Order>();
            var order = await orderDbSet.GetAsync(orderId);
            if (order == null)
            {
                return Result.Fail(new Error("Order not found").WithMetadata("Code", "OrderNotFound"));
            }

            if (order.ExpireAt < DateTime.UtcNow)
            {
                return Result.Fail(new Error("Order already expire").WithMetadata("Code", "OrderAlreadyExpire"));
            }

            var timeLeft = order.ExpireAt - DateTime.UtcNow;
            return Result.Ok(timeLeft);
        }

        public async Task<Result<IEnumerable<OrderListItemDto>>> GetOrdersByUserAsync(string userId)
        {
            var orderRepository = _unitOfWork.GetOrderRepository();

            var orders = await orderRepository.GetOrdersByUserAsync(userId);

            return Result.Ok(orders);
        }

        public async Task<Result<OrderListItemDto>> CheckOrderQRTokenAsync(Guid qrToken)
        {
            var orderRepository = _unitOfWork.GetOrderRepository();

            var order = await orderRepository.GetOrderByQrTokenAsync(qrToken);

            if (order == null)
            {
                return Result.Fail(new Error("Order not found").WithMetadata("Code", "OrderNotFound"));
            }

            if (order.IsQrTokenUsed)
            {
                return Result.Fail(new Error("QR code already used").WithMetadata("Code", "QRCodeAlreadyUsed"));
            }
            var o = await orderRepository.GetAsync(order.OrderId);
            if (o == null)
            {
                return Result.Fail(new Error("Order not found").WithMetadata("Code", "OrderNotFound"));
            }
            o.IsQrTokenUsed = true;
            o.QrCodeUsingDateTime = DateTime.UtcNow;
            await _unitOfWork.CommitAsync();

            return order;
        }
    }
}
