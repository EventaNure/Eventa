using Eventa.Application.Common;
using Eventa.Application.DTOs.Users;
using Eventa.Application.Services;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Eventa.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private const int bookingTimeInMinutes = 1;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public UserService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        public async Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto dto)
        {
            var code = GenerateVerificationCode();
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                VerificationCode = code
            };
            return await RegisterAsync(user, dto.Password, DefaultRoles.UserRole);
        }

        public async Task<Result<RegisterResultDto>> RegisterOrganizerAsync(RegisterOrganizerDto dto)
        {
            var code = GenerateVerificationCode();
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Name = dto.Name,
                VerificationCode = code,
                Organization = dto.Organization
            };
            return await RegisterAsync(user, dto.Password, DefaultRoles.OrganizerRole);
        }

        private async Task<Result<RegisterResultDto>> RegisterAsync(ApplicationUser user, string password, string role)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    return Result.Fail(new Error("User with this email already exists").WithMetadata("Code", "DuplicateEmail"));
                }

                return Result.Fail(new Error("Failed to register user").WithMetadata("Code", "RegistrationFailed"));
            }

            await _userManager.AddToRoleAsync(user, role);
            
            await SendRegistrationEmailAsync(user.Email!, user.VerificationCode!);

            return Result.Ok(new RegisterResultDto
            {
                UserId = user.Id
            });
        }

        public async Task<Result> ResendRegistrationEmailAsync(string userId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

            var code = GenerateVerificationCode();
            user.VerificationCode = code;
            await _userManager.UpdateAsync(user);

            await SendRegistrationEmailAsync(user.Email!, code);

            return Result.Ok();

        }

        public async Task<Result<ConfirmEmailResultDto>> ConfirmEmailAsync(ConfirmEmailDto dto)
        {
            var getUserResult = await GetUserAsync(dto.UserId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

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

            return Result.Ok(new ConfirmEmailResultDto
            {
                Role = (await _userManager.GetRolesAsync(user)).First()
            });
        }

        public async Task<Result<LoginResultDto>> LoginAsync(LoginUserDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }
            var result = await _signInManager.PasswordSignInAsync(user, dto.Password, true, false);
            bool emailConfirmed = true;
            if (!result.Succeeded)
            {
                if (user.EmailConfirmed)
                {
                    return Result.Fail(new Error("Login or password incorrect").WithMetadata("Code", "LoginFailed"));
                }

                var code = GenerateVerificationCode();
                user.VerificationCode = code;
                await _userManager.UpdateAsync(user);

                await SendRegistrationEmailAsync(dto.Email, code);

                emailConfirmed = false;
            }

            return Result.Ok(new LoginResultDto { 
                UserId = user.Id, 
                EmailConfirmed = emailConfirmed, 
                Role = (await _userManager.GetRolesAsync(user)).First() 
            });
        }

        private async Task<Result<ApplicationUser>> GetUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }

            return Result.Ok(user);
        }

        public async Task<Result<DateTime>> GetBookingDateTimeExpireAsync(string userId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

            if (user.TicketsExpireAt <= DateTime.UtcNow)
            {
                return Result.Fail(new Error("Cart is empty").WithMetadata("Code", "CartEmpty"));
            }

            return user.TicketsExpireAt;
        }

        public async Task<Result<TimeSpan>> GetBookingTimeLeftAsync(string userId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

            if (user.TicketsExpireAt <= DateTime.UtcNow)
            {
                return Result.Fail(new Error("Tickets not booked").WithMetadata("Code", "TicketsNotBooked"));
            }

            return user.TicketsExpireAt - DateTime.UtcNow;
        }

        public async Task<Result> ChangeBookingExpireTimeAsync(string userId, int eventDateTimeId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

            bool isUpdated = false;

            if (user.TicketsExpireAt <= DateTime.UtcNow)
            {
                user.TicketsExpireAt = DateTime.UtcNow + TimeSpan.FromMinutes(bookingTimeInMinutes);
                isUpdated = true;
            }

            if (user.EventDateTimeId != eventDateTimeId)
            {
                user.EventDateTimeId = eventDateTimeId;
                isUpdated = true;
            }

            if (isUpdated)
            {
                await _userManager.UpdateAsync(user);
            }

            return Result.Ok();
        }

        private string GenerateVerificationCode()
        {
            Random random = new();
            return random.Next(1000000).ToString("D6");
        }

        private async Task SendRegistrationEmailAsync(string email, string code)
        {
            await _emailSender.SendEmailAsync(
                email,
                "Registration confirmation",
                $"Confirm email to register in Eventa. Confirmation code: {code}");
        }
    }
}
