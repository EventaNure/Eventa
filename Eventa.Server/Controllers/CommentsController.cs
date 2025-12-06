using System.Security.Claims;
using AutoMapper;
using Eventa.Application.DTOs.Comments;
using Eventa.Application.Services.Comments;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public CommentsController(ICommentService commentService, IMapper mapper)
        {
            _commentService = commentService;
            _mapper = mapper;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(CommentDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateComment(CreateCommentRequestModel requestModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _commentService.CreateCommentAsync(userId, _mapper.Map<CreateCommentDto>(requestModel));
            if (!result.IsSuccess)
            {
                return BadRequest(result.Errors);
            }

            return Ok(result.Value);
        }
    }
}
