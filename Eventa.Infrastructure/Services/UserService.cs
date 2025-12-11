using System.Security.Claims;
using Eventa.Application.Common;
using Eventa.Application.DTOs.Comments;
using Eventa.Application.DTOs.Users;
using Eventa.Application.Services;
using FluentResults;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Eventa.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private const int bookingTimeInMinutes = 1;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public UserService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender)
        {
            _dbContext = dbContext;
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

            return Result.Ok(new LoginResultDto
            {
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

            if (user.TicketsExpireAt == null || user.TicketsExpireAt <= DateTime.UtcNow)
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

            if (user.TicketsExpireAt == null || user.TicketsExpireAt <= DateTime.UtcNow)
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

            if (user.TicketsExpireAt == null || user.TicketsExpireAt <= DateTime.UtcNow)
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

        public async Task<Result> DeleteInformationAboutCartAsync(string userId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }

            var user = getUserResult.Value;

            user.EventDateTimeId = null;
            user.TicketsExpireAt = null;
            await _userManager.UpdateAsync(user);

            return Result.Ok();
        }

        public async Task<Result<ExternalLoginResultDto>> HandleGoogleLoginAsync(string idToken, string? role)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            }
            catch (InvalidJwtException)
            {
                return Result.Fail(new Error("Invalid or expired Google token").WithMetadata("Code", "InvalidToken"));
            }

            var user = await _userManager.FindByLoginAsync("Google", payload.Subject);
            var isNewUser = false;
            if (user == null)
            {
                user = await _userManager.FindByEmailAsync(payload.Email);
                if (user == null)
                {
                    if (role == null)
                    {
                        return Result.Fail(new Error("Role must be specified for new users").WithMetadata("Code", "RoleNotSpecified"));
                    }

                    if (role != DefaultRoles.UserRole && role != DefaultRoles.OrganizerRole)
                    {
                        return Result.Fail(new Error("Invalid role specified").WithMetadata("Code", "InvalidRole"));
                    }

                    isNewUser = true;
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        EmailConfirmed = true,
                        Name = payload.Name
                    };
                    await _userManager.CreateAsync(user);
                    await _userManager.AddToRoleAsync(user, role);
                }
                var info = new UserLoginInfo("Google", payload.Subject, "Google");
                if (info == null)
                {
                    return Result.Fail(new Error("Error loading external login information").WithMetadata("Code", "ExternalLoginInfoError"));
                }
                await _userManager.AddLoginAsync(user, info);
            }

            await _signInManager.SignInAsync(user, true);

            return Result.Ok(new ExternalLoginResultDto
            {
                IsLogin = !isNewUser,
                UserId = user.Id,
                Name = user.Name,
                Role = role ?? (await _userManager.GetRolesAsync(user)).First()
            });
        }

        public async Task<Result> SetPersonalUserDataAsync(string userId, PersonalUserDataDto dto)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }
            var user = getUserResult.Value;
            user.Name = dto.Name;
            await _userManager.UpdateAsync(user);
            return Result.Ok();
        }

        public async Task<Result> SetOrganizerUserDataAsync(string userId, PersonalOrganizerDataDto dto)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }
            var user = getUserResult.Value;
            user.Name = dto.Name;
            user.Organization = dto.Organization;
            await _userManager.UpdateAsync(user);
            return Result.Ok();
        }

        public async Task<Result<string>> GetUserNameAsync(string userId)
        {
            var getUserResult = await GetUserAsync(userId);
            if (!getUserResult.IsSuccess)
            {
                return Result.Fail(getUserResult.Errors[0]);
            }
            var user = getUserResult.Value;

            return Result.Ok(user.Name);
        }

        public async Task<Result<UserProfileDataDto>> GetPersonalUserDataAsync(string userId)
        {
            var presonalData = await _dbContext.Users
                .Where(u => u.Id == userId)
                .Select(u => new UserProfileDataDto
                {
                    Name = u.Name,
                    Email = u.Email!,
                    Organization = u.Organization,
                    Rating = u.Events.SelectMany(e => e.EventDateTimes.Where(edt => edt.Event.ApplicationUserId == e.ApplicationUserId)
                        .SelectMany(edt => edt.Orders
                            .Where(o => o.IsPurcharsed && o.Comment != null)
                            .Select(o => (double?)o.Comment!.Rating)
                        ))
                        .Average() ?? 0,
                    CommentsAboutMe = u.Events.SelectMany(e => e.EventDateTimes
                        .SelectMany(edt =>
                            edt.Orders
                            .Where(o => o.IsPurcharsed && o.Comment != null)
                            .Select(o => new CommentDto
                            {
                                Id = o.Comment!.Id,
                                UserName = _dbContext.Users
                                    .Where(u => u.Id == o.UserId)
                                    .Select(u => u.Name)
                                    .First(),
                                Rating = o.Comment.Rating,
                                Content = o.Comment.Content,
                                CreationDateTime = o.Comment.CreatedAt
                            })
                         ))
                         .ToList(),
                    MyComments = _dbContext.Comments
                        .Where(c => c.UserId == u.Id)
                        .Select(c => new CommentDto
                        {
                            Id = c.Id,
                            UserName = _dbContext.Users
                                .Where(user => user.Id == c.Order.EventDateTime.Event.ApplicationUserId)
                                .Select(user => user.Name)
                                .First(),
                            Rating = c.Rating,
                            Content = c.Content,
                            CreationDateTime = c.CreatedAt
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (presonalData == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }

            return Result.Ok(presonalData);
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
