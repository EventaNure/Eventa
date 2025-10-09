using System.Text;
using Eventa.Application.DTOs;
using Eventa.Application.Services;
using FluentResults;
using Microsoft.AspNetCore.Http.HttpResults;
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
            Random random = new Random();
            var code = random.Next(1000000).ToString("D6");
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                VerificationCode = code
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

            return Result.Ok(new EmailConfirmationDto
            {
                UserId = user.Id,
                Code = code
            });
        }

        public async Task<Result> ConfirmEmailAsync(EmailConfirmationDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.UserId);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }

            if (user.EmailConfirmed)
            {
                return Result.Fail(new Error("Email already confirmed").WithMetadata("Code", "EmailAlreadyConfirmed"));
            }

            if (user.VerificationCode != dto.Code)
            {
                return Result.Fail(new Error("Token incorrect").WithMetadata("Code", "TokenIncorrect"));
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);
            await _signInManager.SignInAsync(user, true);

            return Result.Ok();
        }

        public async Task<Result> LoginAsync(LoginUserDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }
            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, true, false);
            if (!result.Succeeded)
            {
                return Result.Fail(new Error("Login or password incorrect").WithMetadata("Code", "LoginFailed"));
            }

            return Result.Ok();
        }
    }
}
