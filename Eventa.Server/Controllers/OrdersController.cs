using System.Security.Claims;
using Eventa.Application.Services.Orders;
using Eventa.Infrastructure.Options;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly PaymentOptions _options;

        public OrdersController(IOrderService paymentService, IOptions<PaymentOptions> options)
        {
            _orderService = paymentService;
            _options = options.Value;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentIntent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var createOrderResult = await _orderService.CreateOrderAsync(userId);
            if (!createOrderResult.IsSuccess)
            {
                return BadRequest(createOrderResult.Errors);
            }

            return Ok(createOrderResult.Value);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> WebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var signature = Request.Headers[_options.Signature].FirstOrDefault();

            if (signature == null)
            {
                return BadRequest();
            }

            var result = await _orderService.HookAsync(json, signature);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok();
        }

        [HttpGet("by-user")]
        [Authorize]
        public async Task<IActionResult> GetOrdersByUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _orderService.GetOrdersByUserAsync(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }

        [HttpGet("QR-token")]
        [Authorize]
        public async Task<IActionResult> GenerateQRToken(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _orderService.GenerateQRTokenAsync(orderId, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }

        [HttpGet("check-QR-token")]
        [Authorize]
        public async Task<IActionResult> CheckQRToken(Guid qrToken)
        {
            var result = await _orderService.CheckOrderQRTokenAsync(qrToken);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }
    }
}
