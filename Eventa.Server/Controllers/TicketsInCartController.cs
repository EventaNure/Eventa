using System.Security.Claims;
using Eventa.Application.Common;
using Eventa.Application.Services.TicketsInCart;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsInCartController : ControllerBase
    {
        private readonly ITicketInCartService _ticketInCartService;

        public TicketsInCartController(ITicketInCartService ticketInCartService) {
            _ticketInCartService = ticketInCartService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddTicketInCart(BookTicketRequestModel requestModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _ticketInCartService.BookTicketAsync(requestModel.EventDateTimeId, requestModel.SeatId, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }

        [HttpGet("by-user")]
        [Authorize]
        public async Task<IActionResult> GetTicketsInCartByUser()
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var getCartsResult = await _ticketInCartService.GetCartsByUserAsync(userId);
            if (!getCartsResult.IsSuccess)
            {
                return BadRequest(getCartsResult.Errors);
            }

            return Ok(getCartsResult.Value);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteTicketInCart(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _ticketInCartService.DeleteTicketAsync(id, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }
    }
}
