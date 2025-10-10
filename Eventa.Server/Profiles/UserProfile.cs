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
            CreateMap<ConfirmEmailDto, EmailConfirmationRequestModel>();
            CreateMap<EmailConfirmationRequestModel, ConfirmEmailDto>();
            CreateMap<ConfirmEmailDto, RegisterResponseModel>();
            CreateMap<LoginRequestModel, LoginUserDto>();
            CreateMap<RegisterResultDto, RegisterResponseModel>();
        }
    }
}
