using AutoMapper;
using Eventa.Application.DTOs.Tags;
using Eventa.Server.ResponseModels;

namespace Eventa.Server.Profiles
{
    public class TagProfile : Profile
    {
        public TagProfile()
        {
            CreateMap<TagDto, TagResponseModel>();
        }
    }
}
