using Eventa.Application.DTOs;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result<ConfirmEmailResultDto>> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<Result<LoginResultDto>> LoginAsync(LoginUserDto dto);
        Task<Result<RegisterResultDto>> RegisterOrganizerAsync(RegisterOrganizerDto dto);
        Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto dto);
        Task<Result> ResendRegistrationEmailAsync(string userId);
    }
}