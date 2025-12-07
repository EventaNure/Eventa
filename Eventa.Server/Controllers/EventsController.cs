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
        [Authorize(Roles = DefaultRoles.OrganizerRole)]
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

        [HttpPost("temp-image")]
        [Authorize(Roles = DefaultRoles.OrganizerRole)]
        public async Task<IActionResult> LoadImage(IFormFile imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            using var imageBytes = imageFile.OpenReadStream();

            var result = await _eventService.LoadImageAsync(imageBytes, imageFile.FileName);
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result.Value);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = DefaultRoles.OrganizerRole)]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] EventRequestModel eventRequestModel)
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
                    return NotFound(result.Errors[0]);
                }

                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = DefaultRoles.OrganizerRole)]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _eventService.DeleteEventAsync(id, userId);
            if (!result.IsSuccess)
            {
                if (result.Errors.Any(e => (string)e.Metadata["Code"] == "EventNotFound"))
                {
                    return NotFound(result.Errors[0]);
                }

                return BadRequest(result.Errors);
            }
            return Ok();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var getEventResult = await _eventService.GetEventAsync(id);

            if (!getEventResult.IsSuccess)
            {
                return NotFound(getEventResult.Errors);
            }

            return Ok(getEventResult.Value);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(int pageNumber = 1, int pageSize = 10, [FromQuery] int[] tagIds = null!, DateOnly? startDate = null!, DateOnly? endDate = null!, string? subName = null!)
        {
            tagIds = tagIds ?? Array.Empty<int>();
            var events = await _eventService.GetEventsAsync(pageNumber, pageSize, tagIds, startDate, endDate, subName);

            return Ok(_mapper.Map<List<EventListItemResponseModel>>(events));
        }

        [HttpGet("by-organizer")]
        [Authorize(Roles = DefaultRoles.OrganizerRole)]
        public async Task<IActionResult> GetEventsByOrganizer(int pageNumber = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var events = await _eventService.GetEventsByOrganizerAsync(pageNumber, pageSize, userId);

            return Ok(_mapper.Map<List<EventListItemResponseModel>>(events));
        }

        [HttpGet("pending")]
        [Authorize(Roles = DefaultRoles.AdminRole)]
        public async Task<IActionResult> GetPendingEvents(int pageNumber = 1, int pageSize = 10)
        {
            var result = await _eventService.GetPendingEventsAsync(pageNumber, pageSize);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            var events = result.Value;

            return Ok(events);
        }

        [HttpPut("{eventId}/approve")]
        [Authorize(Roles = DefaultRoles.AdminRole)]
        public async Task<IActionResult> ApproveEvent(int eventId)
        {
            var result = await _eventService.ApproveEventAsync(eventId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }

        [HttpPut("{eventId}/deny")]
        [Authorize(Roles = DefaultRoles.AdminRole)]
        public async Task<IActionResult> DenyEvent(int eventId)
        {
            var result = await _eventService.DenyEventAsync(eventId);

            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
    }
}
