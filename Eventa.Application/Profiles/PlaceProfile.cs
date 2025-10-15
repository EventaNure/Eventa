using AutoMapper;
using Eventa.Application.DTOs.Places;
using Eventa.Domain;

namespace Eventa.Application.Profiles
{
    public class PlaceProfile : Profile
    {
        public PlaceProfile()
        {
            CreateMap<Place, PlaceDto>();
        }
    }
}
