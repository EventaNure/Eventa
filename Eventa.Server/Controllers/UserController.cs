using AutoMapper;
using Eventa.Application.DTOs;
using Eventa.Application.Services;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace Eventa.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailSender _emailSender;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IEmailSender emailSender, IMapper mapper)
        {
            _userService = userService;
            _emailSender = emailSender;
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

            await _emailSender.SendEmailAsync(
                request.Email, 
                "Registration confirmation", 
                $"Confirm email to register in Eventa. Confirmation code: {registerResult.Value.Code}");

            return Ok(_mapper.Map<RegisterResponseModel>(registerResult.Value));
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmationRequestModel request)
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

                if (confirmEmailResult.Errors.Any(e => (string)e.Metadata["Code"] == "EmailAlreadyConfirmed"))
                {
                    return BadRequest(new { message = "Email already confirmed" });
                }

                return BadRequest(new { message = "Token incorrect" });
            }

            return Ok(new { message = "Your email successful confirmed" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestModel request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var loginResultResult = await _userService.LoginAsync(_mapper.Map<LoginUserDto>(request));

            if (!loginResultResult.IsSuccess)
            {
                return BadRequest(new { message = "Email or password incorrect" });
            }

            return Ok();
        }
    }
}
