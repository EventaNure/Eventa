using AutoMapper;
using Eventa.Application.DTOs;
using Eventa.Server.RequestModels;
using Eventa.Server.ResponseModels;

namespace Eventa.Server.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<RegisterRequestModel, RegisterUserDto>();
            CreateMap<EmailConfirmationDto, EmailConfirmationRequestModel>();
            CreateMap<EmailConfirmationRequestModel, EmailConfirmationDto>();
            CreateMap<EmailConfirmationDto, RegisterResponseModel>();
        }
    }
}
