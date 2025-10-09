using Eventa.Application.DTOs;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result> ConfirmEmailAsync(EmailConfirmationDto dto);
        Task<Result<EmailConfirmationDto>> Register(RegisterUserDto dto);
    }
}