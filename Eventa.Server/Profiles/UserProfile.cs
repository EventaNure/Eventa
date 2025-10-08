using AutoMapper;
using Eventa.Application.DTOs;
using Eventa.Server.RequestModels;

namespace Eventa.Server.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile() {
            CreateMap<RegisterRequestModel, RegisterUserDto>();
        }
    }
}
