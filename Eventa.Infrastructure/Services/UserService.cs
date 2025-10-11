﻿using Eventa.Application.Common;
using Eventa.Application.DTOs;
using Eventa.Application.Services;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace Eventa.Infrastructure.Services
{
    public class UserService : IUserService
    {
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
            return await RegisterAsync(user, dto.Password, Roles.UserRole);
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
            return await RegisterAsync(user, dto.Password, Roles.OrganizerRole);
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
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Result.Fail(new Error("User not found").WithMetadata("Code", "UserNotFound"));
            }

            var code = GenerateVerificationCode();
            user.VerificationCode = code;
            await _userManager.UpdateAsync(user);

            await SendRegistrationEmailAsync(user.Email!, code);

            return Result.Ok();

        }

        public async Task<Result<ConfirmEmailResultDto>> ConfirmEmailAsync(ConfirmEmailDto dto)
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
