using AutoMapper;
using Eventa.Application.Services.Events;
using Eventa.Application.Services.Sections;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly ISeatService _seatService;
        private readonly IMapper _mapper;

        public SeatsController(ISeatService sectionService, IMapper mapper)
        {
            _seatService = sectionService;
            _mapper = mapper;
        }

        [HttpGet("free-with-hall-plan")]
        public async Task<IActionResult> GetFreeSeatsWithHallPlan(int eventId)
        {
            var sectionType = await _seatService.GetFreeSeatsWithHallPlan(eventId);

            return Ok(sectionType.Value);
        }
    }
}
