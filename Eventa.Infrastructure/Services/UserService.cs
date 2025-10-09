using System.Text;
using Eventa.Application.DTOs;
using Eventa.Application.Services;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace Eventa.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Result<EmailConfirmationDto>> RegisterAsync(RegisterUserDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
            };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    return Result.Fail(new Error("User with this email already exists").WithMetadata("Code", "DuplicateEmail"));
                }

                return Result.Fail(new Error("Failed to register user").WithMetadata("Code", "RegistrationFailed"));
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return new EmailConfirmationDto
            {
                UserId = user.Id,
                Code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code))
            };
        }

        public async Task<Result> ConfirmEmailAsync(EmailConfirmationDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }
            var result = await _userManager.ConfirmEmailAsync(user, 
                Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Code)));
            if (!result.Succeeded)
            {
                return Result.Fail(new Error("Token incorrect").WithMetadata("Code", "TokenIncorrect"));
            }
            await _signInManager.SignInAsync(user, true);
            return Result.Ok();
        }
    }
}
