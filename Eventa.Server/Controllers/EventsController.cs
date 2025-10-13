using System.Security.Claims;
using AutoMapper;
using Eventa.Application.Common;
using Eventa.Application.DTOs.Events;
using Eventa.Application.Services.Events;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IMapper _mapper;

        public EventsController(IEventService eventService, IMapper mapper)
        {
            _eventService = eventService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.OrganizerRole)]
        public Task<IActionResult> CreateEvent()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            throw new NotImplementedException();
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int[] tagIds = null!)
        {
            IEnumerable<EventListItemDto> events;
            if (tagIds == null || !tagIds.Any())
            {
                events = await _eventService.GetEventsAsync(pageNumber, pageSize);
            } else
            {
                events = await _eventService.GetEventsByTagsAsync(pageNumber, pageSize, tagIds.ToList());
            }


            return Ok(_mapper.Map<List<EventListItemResponseModel>>(events));
        }
    }
}
