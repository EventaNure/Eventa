using Eventa.Application.DTOs.Users;
using FluentResults;

namespace Eventa.Application.Services
{
    public interface IUserService
    {
        Task<Result> ChangeBookingExpireTimeAsync(string userId, int eventDateTime);
        Task<Result<ConfirmEmailResultDto>> ConfirmEmailAsync(ConfirmEmailDto dto);
        Task<Result> DeleteInformationAboutCartAsync(string userId);
        Task<Result<DateTime>> GetBookingDateTimeExpireAsync(string userId);
        Task<Result<TimeSpan>> GetBookingTimeLeftAsync(string userId);
        Task<Result<UserProfileDataDto>> GetPersonalUserDataAsync(string userId);
        Task<Result<ExternalLoginResultDto>> HandleOrganizerGoogleLoginAsync(string idToken);
        Task<Result<ExternalLoginResultDto>> HandleUserGoogleLoginAsync(string idToken);
        Task<Result<LoginResultDto>> LoginAsync(LoginUserDto dto);
        Task<Result<RegisterResultDto>> RegisterOrganizerAsync(RegisterOrganizerDto dto);
        Task<Result<RegisterResultDto>> RegisterUserAsync(RegisterUserDto dto);
        Task<Result> ResendRegistrationEmailAsync(string userId);
        Task<Result> SetOrganizerUserDataAsync(string userId, PersonalOrganizerDataDto dto);
        Task<Result> SetPersonalUserDataAsync(string userId, PersonalUserDataDto dto);
    }
}