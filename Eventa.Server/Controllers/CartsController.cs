using System.Security.Claims;
using Eventa.Application.Common;
using Eventa.Application.Services.Carts;
using Eventa.Server.RequestModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService) {
            _cartService = cartService;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> BookTicket(BookTicketRequestModel requestModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _cartService.BookTicket(requestModel.EventId, requestModel.SeatId, userId);
            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok();
        }
    }
}
