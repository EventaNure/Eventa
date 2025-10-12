using AutoMapper;
using Eventa.Application.Services.Tags;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TagsController : ControllerBase
    {
        private readonly ITagService _tagService;
        private readonly IMapper _mapper;

        public TagsController(ITagService eventService, IMapper mapper)
        {
            _tagService = eventService;
            _mapper = mapper;
        }

        [HttpGet("main")]
        public async Task<IActionResult> GetMainTags()
        {
            var tags = await _tagService.GetMainTagsAsync();

            return Ok(_mapper.Map<IEnumerable<TagResponseModel>>(tags));
        }

        [HttpGet]
        public async Task<IActionResult> GetTags()
        {
            var tags = await _tagService.GetTagsAsync();

            return Ok(_mapper.Map<IEnumerable<TagResponseModel>>(tags));
        }
    }
}
