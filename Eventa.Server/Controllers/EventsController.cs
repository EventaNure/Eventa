using System.Security.Claims;
using AutoMapper;
using Eventa.Application.Common;
using Eventa.Application.DTOs.Events;
using Eventa.Application.Services.Events;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

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
        public async Task<IActionResult> CreateEvent(EventRequestModel eventRequestModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var dto = _mapper.Map<CreateEventDto>(eventRequestModel);
            dto.OrganizerId = userId;
            dto.ImageBytes = eventRequestModel.ImageFile.OpenReadStream();
            dto.ImageFileName = eventRequestModel.ImageFile.FileName;
            var result = await _eventService.CreateEventAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.OrganizerRole)]
        public async Task<IActionResult> UpdateEvent(int id, EventRequestModel eventRequestModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var dto = _mapper.Map<UpdateEventDto>(eventRequestModel);
            dto.OrganizerId = userId;
            dto.EventId = id;
            dto.ImageBytes = eventRequestModel.ImageFile.OpenReadStream();
            dto.ImageFileName = eventRequestModel.ImageFile.FileName;
            var result = await _eventService.UpdateEventAsync(dto);
            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => (string)e.Metadata["Code"] == "EventNotFound"))
                {
                    return Conflict(result.Errors[0]);
                }

                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = Roles.OrganizerRole)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _eventService.DeleteEventAsync(id, userId);
            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => (string)e.Metadata["Code"] == "EventNotFound"))
                {
                    return Conflict(result.Errors[0]);
                }

                return BadRequest(result.Errors);
            }
            return Ok();
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
