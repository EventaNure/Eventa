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

        public async Task<Result<EmailConfirmationDto>> Register(RegisterUserDto dto)
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
    }
}
