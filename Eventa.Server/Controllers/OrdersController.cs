using System.Security.Claims;
using Eventa.Application.Services.Orders;
using Eventa.Infrastructure.Options;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly PaymentOptions _options;

        public OrdersController(IOrderService paymentService, IOptions<PaymentOptions> options, IWebHostEnvironment webHostEnvironment)
        {
            _orderService = paymentService;
            _webHostEnvironment = webHostEnvironment;
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

            return Ok(new GenerateQrTokenResultResponseModel
            {
                CheckQrTokenUrl = Url.Action(
                    nameof(CheckQRToken),
                    nameof(OrdersController).Replace("Controller", ""),
                    new { qrToken = result.Value.QrToken},
                    Request.Scheme),
                IsQrTokenUsed = result.Value.IsQrTokenUsed,
                QrCodeUsingDateTime = result.Value.QrCodeUsingDateTime
            });
        }

        [HttpGet("check-QR-token")]
        public async Task<IActionResult> CheckQRToken(Guid qrToken)
        {
            var result = await _orderService.CheckOrderQRTokenAsync(qrToken);
            if (!result.IsSuccess)
            {
                string errorCheckPath = Path.Combine(_webHostEnvironment.WebRootPath, "pages", "error-check.html");
                string errorCheckHtml = System.IO.File.ReadAllText(errorCheckPath);
                errorCheckHtml = errorCheckHtml.Replace("{ErrorMessage}", result.Errors.FirstOrDefault()?.Message ?? string.Empty);

                return Content(errorCheckHtml, "text/html");
            }

            string validCheckPath = Path.Combine(_webHostEnvironment.WebRootPath, "pages", "valid-check.html");

            string ticketPath = Path.Combine(_webHostEnvironment.WebRootPath, "pages", "ticket.html");

            string checkHtml = System.IO.File.ReadAllText(validCheckPath);

            string ticketHtml = System.IO.File.ReadAllText(ticketPath);

            StringBuilder ticketsHtml = new StringBuilder();

            foreach (var ticket in result.Value.Tickets)
            {
                ticketsHtml.Append(ticketHtml
                    .Replace("{RowType}", ticket.RowTypeName.ToString())
                    .Replace("{Row}", ticket.Row.ToString())
                    .Replace("{SeatNumber}", ticket.SeatNumber.ToString()));
            }

            checkHtml = checkHtml
                .Replace("{EventName}", result.Value.EventName)
                .Replace("{EventDateTime}", result.Value.EventDateTime.ToString("dd-MM-yyyy hh:mm"))
                .Replace("{Tickets}", ticketsHtml.ToString());

            return Content(checkHtml, "text/html");
        }
    }
}
