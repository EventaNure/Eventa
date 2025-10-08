using Eventa.Application.DTOs;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result<EmailConfirmationDto>> Register(RegisterUserDto dto);
    }
}