using AutoMapper;
using Eventa.Application.DTOs.Places;
using Eventa.Server.ResponseModels;

namespace Eventa.Server.Profiles
{
    public class PlaceProfile : Profile
    {
        public PlaceProfile()
        {
            CreateMap<PlaceDto, PlaceListItemResponseModel>();
        }
    }
}
