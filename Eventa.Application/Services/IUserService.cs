using Eventa.Application.DTOs.Users;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result> ChangeBookingExpireTimeAsync(string userId, int eventDateTime);
        Task<Result<ConfirmEmailResultDto>> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<Result<DateTime>> GetBookingDateTimeExpireAsync(string userId);
        Task<Result<TimeSpan>> GetBookingTimeLeftAsync(string userId);
        Task<Result<LoginResultDto>> LoginAsync(LoginUserDto dto);
        Task<Result<RegisterResultDto>> RegisterOrganizerAsync(RegisterOrganizerDto dto);
        Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto dto);
        Task<Result> ResendRegistrationEmailAsync(string userId);
    }
}