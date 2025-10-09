using Eventa.Application.DTOs;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result> ConfirmEmailAsync(EmailConfirmationDto dto);
        Task<Result> LoginAsync(LoginUserDto dto);
        Task<Result<EmailConfirmationDto>> RegisterAsync(RegisterUserDto dto);
    }
}