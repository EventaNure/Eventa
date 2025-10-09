using AutoMapper;
using Eventa.Application.DTOs;
using Eventa.Application.Services;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var registerResult = await _userService.RegisterAsync(_mapper.Map<RegisterUserDto>(request));

            if (!registerResult.IsSuccess)
            {
                if (registerResult.Errors.Any(e => (string)e.Metadata["Code"] == "DuplicateEmail"))
                {
                    return Conflict(new {message = "Email already exists"});
                }

                return BadRequest(new {message = "Failed to register user" });
            }

            return Ok();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmationResponseModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var confirmEmailResult = await _userService.ConfirmEmailAsync(_mapper.Map<EmailConfirmationDto>(request));

            if (!confirmEmailResult.IsSuccess)
            {
                if (confirmEmailResult.Errors.Any(e => (string)e.Metadata["Code"] == "UserNotFound"))
                {
                    return NotFound(new { message = "User not found" });
                }

                return BadRequest(new { message = "Token incorrect" });
            }

            return Ok(new { message = "Your email successful confirmed" });
        }
    }
}
