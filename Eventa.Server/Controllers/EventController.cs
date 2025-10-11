using System.Security.Claims;
using Eventa.Application.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        [HttpPost("create")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.OrganizerRole)]
        public Task<IActionResult> CreateEvent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            throw new NotImplementedException();
        }
    }
}
